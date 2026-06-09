using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Services;

public sealed class FeedbackComposer : IFeedbackComposer
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly IEvaluationRepository _repo;
    private readonly IFeedbackPromptBuilder _feedbackPromptBuilder;
    private readonly ILogger<FeedbackComposer> _logger;

    private const string ModelEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public FeedbackComposer(
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        IEvaluationRepository repo,
        IFeedbackPromptBuilder feedbackPromptBuilder,
        ILogger<FeedbackComposer> logger
    )
    {
        _config = config;
        _httpClient = httpClientFactory.CreateClient();
        _repo = repo;
        _feedbackPromptBuilder = feedbackPromptBuilder;
        _logger = logger;
    }

    public async Task<PracticeFeedbackResponseDto> ComposeAsync(
        PracticeSession session,
        Evaluation evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning> warnings,
        CancellationToken ct = default
    )
    {
        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);
        var patient = await _repo.GetVirtualPatientByIdAsync(session.PatientId);

        var prompt = _feedbackPromptBuilder.Build(
            session,
            evaluation,
            epaScores,
            warnings,
            clinicalDx?.CanonicalDiagnosis ?? string.Empty,
            clinicalDx?.DescriptionText ?? string.Empty,
            patient?.TimeSettingMinutes ?? 30,
            patient?.ArgumentTimeMinutes ?? 15
        );
        var apiKey = _config["GeminiAi:ApiKey"] ?? _config["GEMINI_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key missing — returning fallback feedback.");
            return BuildFallbackFeedback(evaluation);
        }

        return await CallGeminiAsync(prompt, apiKey, evaluation, ct);
    }

    private async Task<PracticeFeedbackResponseDto> CallGeminiAsync(
        string prompt,
        string apiKey,
        Evaluation evaluation,
        CancellationToken ct
    )
    {
        var url = $"{ModelEndpoint}?key={apiKey}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" },
        };

        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );
            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(raw);
            var text = doc
                .RootElement.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                return BuildFallbackFeedback(evaluation);

            return ParseFeedback(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini feedback generation failed.");
            return BuildFallbackFeedback(evaluation);
        }
    }

    private static PracticeFeedbackResponseDto ParseFeedback(string raw)
    {
        var clean = raw.Trim();

        if (clean.StartsWith("```"))
        {
            var firstNewLine = clean.IndexOf('\n');
            var lastBackticks = clean.LastIndexOf("```");
            if (firstNewLine >= 0 && lastBackticks > firstNewLine)
            {
                clean = clean.Substring(firstNewLine + 1, lastBackticks - firstNewLine - 1).Trim();
            }
        }

        var start = clean.IndexOf('{');
        var end = clean.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            clean = clean.Substring(start, end - start + 1);
        }

        using var payload = JsonDocument.Parse(clean);
        var root = payload.RootElement;
        var strength = ReadFlexibleText(root, "strength");
        var weakness = ReadFlexibleText(root, "weakness");
        var overallAttemptFeedback = ReadFlexibleText(root, "overallAttemptFeedback");
        var overallLabel = ReadFlexibleText(root, "overallLabel");

        return new PracticeFeedbackResponseDto
        {
            Strength = strength,
            Improvement = weakness,
            OverallAttempt = overallAttemptFeedback,
            OverallLabel = string.IsNullOrWhiteSpace(overallLabel) ? "DEVELOPING" : overallLabel,
        };
    }

    private static string? ReadFlexibleText(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
            return null;

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()?.Trim(),
            JsonValueKind.Array => string.Join(
                "\n",
                element
                    .EnumerateArray()
                    .Select(ToText)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
            ),
            JsonValueKind.Object => ToText(element),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    private static string ToText(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Join(
                " ",
                element
                    .EnumerateArray()
                    .Select(ToText)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
            ),
            JsonValueKind.Object => string.Join(
                " ",
                element
                    .EnumerateObject()
                    .Select(p => ToText(p.Value))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
            ),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty,
        };
    }

    private static PracticeFeedbackResponseDto BuildFallbackFeedback(Evaluation eval)
    {
        var score = (int)(eval.Score ?? 0);
        return new PracticeFeedbackResponseDto
        {
            Strength = "Feedback requires AI service. Check Gemini API configuration.",
            Improvement = "Review your EPA performance breakdown above for specific areas.",
            OverallAttempt =
                $"Session completed with score {score}/110. Detailed coaching unavailable.",
            OverallLabel = score switch
            {
                >= 90 => "EXCELLENT",
                >= 75 => "GOOD",
                >= 60 => "DEVELOPING",
                _ => "NEEDS_IMPROVEMENT",
            },
        };
    }

    private static string Truncate(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "(empty)"
        : text.Length <= max ? text
        : text.Substring(0, Math.Min(text.Length, max)) + "...[truncated]";
}
