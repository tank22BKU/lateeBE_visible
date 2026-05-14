using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Services;

public sealed class FeedbackComposer : IFeedbackComposer
{
    private readonly IConfiguration _config;
    private readonly HttpClient     _httpClient;
    private readonly ILogger<FeedbackComposer> _logger;

    private const string ModelEndpoint =
        "[https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent](https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent)";

    public FeedbackComposer(
        IConfiguration             config,
        IHttpClientFactory        httpClientFactory,
        ILogger<FeedbackComposer> logger)
    {
        _config     = config;
        _httpClient = httpClientFactory.CreateClient();
        _logger     = logger;
    }

    public async Task<PracticeFeedbackResponseDto> ComposeAsync(
        PracticeSession           session,
        Evaluation               evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning>            warnings,
        CancellationToken        ct = default)
    {
        var prompt = BuildPrompt(session, evaluation, epaScores, warnings);
        var apiKey = _config["GeminiAi:ApiKey"] ?? _config["GEMINI_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key missing — returning fallback feedback.");
            return BuildFallbackFeedback(evaluation);
        }

        return await CallGeminiAsync(prompt, apiKey, evaluation, ct);
    }

    private static string BuildPrompt(
        PracticeSession           session,
        Evaluation               evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning>            warnings)
    {
        var warningLabels = warnings.Select(w => w.Label ?? "UNKNOWN").ToList();
        var finalScore    = (int)(evaluation.Score ?? 0);
        var level         = evaluation.EntrustmentLevel ?? 1;

        var epaContext = new StringBuilder();
        foreach (var epa in epaScores)
        {
            epaContext.AppendLine(
                $"- {epa.EpaId}: {epa.NumericalScore}/20 (Level {epa.EntrustmentLevel}) — {epa.FeedbackDetail}");
            if (epa.EvidenceCited.Count > 0)
                epaContext.AppendLine($"  Evidence: {string.Join("; ", epa.EvidenceCited.Take(2))}");
            if (epa.FailurePatterns.Count > 0)
                epaContext.AppendLine($"  Failures: {string.Join(", ", epa.FailurePatterns)}");
        }

        return $$"""
                SYSTEM:
                You are a clinical education coach providing actionable, evidence-based feedback
                to a medical learner after their Virtual Patient simulation.
                Be honest, constructive, specific. Reference EPA evidence. Focus on growth.
                NEVER fabricate clinical details not in the transcripts.

                LEARNER PERFORMANCE:
                Final Score         : {{finalScore}}/110
                Entrustment Level   : {{level}}/5
                Final Diagnosis     : {{session.FinalDiagnosis ?? "Not submitted"}}
                Session Duration    : {{evaluation.Duration ?? 0}} minutes
                Warnings Triggered  : {{string.Join(", ", warningLabels.DefaultIfEmpty("None"))}}

                EPA PERFORMANCE BREAKDOWN:
                {{epaContext}}

                VP CONVERSATION EXCERPT:
                {{Truncate(session.VpConversationLog, 1000)}}

                AI REASONING EXCERPT:
                {{Truncate(session.AiReasoningLog, 1000)}}

                INSTRUCTIONS:
                Return ONLY valid JSON — no markdown, no preamble:
                {
                    "strength"              : "<2–3 specific strengths with transcript evidence>",
                    "weakness"              : "<2–3 areas for improvement with reasoning>",
                    "improvementSuggestion" : "<3 actionable prioritized steps>",
                    "overallAttemptFeedback": "<1–2 paragraph narrative>",
                    "overallLabel"          : "<EXCELLENT|GOOD|DEVELOPING|NEEDS_IMPROVEMENT>"
                }
            """;
    }

    private async Task<PracticeFeedbackResponseDto> CallGeminiAsync(
        string prompt, string apiKey, Evaluation evaluation, CancellationToken ct)
    {
        var url  = $"{ModelEndpoint}?key={apiKey}";
        var body = new
        {
            contents         = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" }
        };

        try
        {
            var content  = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
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
        var end   = clean.LastIndexOf('}');
        if (start >= 0 && end > start) 
        {
            clean = clean.Substring(start, end - start + 1);
        }

        var opts   = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parsed = JsonSerializer.Deserialize<FeedbackSchema>(clean, opts);

        return new PracticeFeedbackResponseDto
        {
            Strength       = parsed?.Strength,
            Improvement    = $"{parsed?.Weakness}\n\nImprovement Steps:\n{parsed?.ImprovementSuggestion}",
            OverallAttempt = parsed?.OverallAttemptFeedback,
            OverallLabel   = parsed?.OverallLabel ?? "DEVELOPING"
        };
    }

    private static PracticeFeedbackResponseDto BuildFallbackFeedback(Evaluation eval)
    {
        var score = (int)(eval.Score ?? 0);
        return new PracticeFeedbackResponseDto
        {
            Strength       = "Feedback requires AI service. Check Gemini API configuration.",
            Improvement    = "Review your EPA performance breakdown above for specific areas.",
            OverallAttempt = $"Session completed with score {score}/110. Detailed coaching unavailable.",
            OverallLabel   = score switch
            {
                >= 90 => "EXCELLENT",
                >= 75 => "GOOD",
                >= 60 => "DEVELOPING",
                _     => "NEEDS_IMPROVEMENT"
            }
        };
    }

    private static string Truncate(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "(empty)"
        : text.Length <= max ? text
        : text.Substring(0, Math.Min(text.Length, max)) + "...[truncated]";

    private sealed class FeedbackSchema
    {
        public string? Strength               { get; set; }
        public string? Weakness               { get; set; }
        public string? ImprovementSuggestion  { get; set; }
        public string? OverallAttemptFeedback { get; set; }
        public string? OverallLabel           { get; set; }
    }
}