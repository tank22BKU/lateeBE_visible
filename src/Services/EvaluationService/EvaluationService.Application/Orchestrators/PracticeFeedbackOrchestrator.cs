using System.Text.Json;
using EvaluationService.Application.Dtos;
using EvaluationService.Application.Services;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Orchestrators;

public sealed class PracticeFeedbackOrchestrator
{
    private readonly IEvaluationRepository _repo;
    private readonly IFeedbackComposer _feedbackComposer;
    private readonly ILogger<PracticeFeedbackOrchestrator> _logger;

    public PracticeFeedbackOrchestrator(
        IEvaluationRepository repo,
        IFeedbackComposer feedbackComposer,
        ILogger<PracticeFeedbackOrchestrator> logger
    )
    {
        _repo = repo;
        _feedbackComposer = feedbackComposer;
        _logger = logger;
    }

    public async Task<PracticeFeedbackResponseDto> GenerateFeedbackAsync(
        string practiceSessionId,
        CancellationToken ct = default
    )
    {
        var existing = await _repo.GetPracticeFeedbackBySessionIdAsync(practiceSessionId);
        if (existing != null && !IsFallbackFeedback(existing))
            return new PracticeFeedbackResponseDto
            {
                Id = existing.Id,
                OverallAttempt = existing.OverallAttempt,
                OverallLabel = existing.OverallLabel,
                Strength = NormalizeStrength(existing.Strength),
                Improvement = NormalizeImprovement(existing.Improvement),
                CreatedAt = existing.CreatedAt,
                WasCached = true,
            };

        var session =
            await _repo.GetPracticeSessionByIdAsync(practiceSessionId)
            ?? throw new InvalidOperationException($"Session '{practiceSessionId}' not found.");

        var clinicalDx = await _repo.GetClinicalDiagnosisByPatientIdAsync(session.PatientId);
        var patient = await _repo.GetVirtualPatientByIdAsync(session.PatientId);

        var evaluations = await _repo.GetByLearnerIdAsync(session.LearnerId);
        var evaluation =
            evaluations.FirstOrDefault(e => e.PracticeSessionId == practiceSessionId)
            ?? throw new InvalidOperationException(
                $"No evaluation for session '{practiceSessionId}'. Submit evaluation first."
            );

        var epaScores = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);
        var warnings = await _repo.GetWarningsByPracticeSessionIdAsync(practiceSessionId);

        _logger.LogInformation(
            "Practice feedback pipeline start: sessionId={SessionId} evaluationId={EvaluationId}",
            practiceSessionId,
            evaluation.Id
        );

        var feedbackDto = await _feedbackComposer.ComposeAsync(
            session,
            evaluation,
            epaScores,
            warnings,
            ct
        );

        var entity = new PracticeFeedback
        {
            Id = Guid.NewGuid().ToString("N"),
            OverallAttempt = feedbackDto.OverallAttempt,
            OverallLabel = feedbackDto.OverallLabel,
            Strength = feedbackDto.Strength,
            Improvement = feedbackDto.Improvement,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EvaluationId = evaluation.Id,
            PracticeSessionId = practiceSessionId,
        };

        await _repo.AddPracticeFeedbackAsync(entity);
        await _repo.SaveChangesAsync();

        feedbackDto.Id = entity.Id;
        feedbackDto.CreatedAt = entity.CreatedAt;
        feedbackDto.WasCached = false;
        return feedbackDto;
    }

    private static bool IsFallbackFeedback(PracticeFeedback feedback)
    {
        var strength = feedback.Strength ?? string.Empty;
        var improvement = feedback.Improvement ?? string.Empty;
        var overallAttempt = feedback.OverallAttempt ?? string.Empty;

        return strength.Contains("Feedback requires AI service", StringComparison.OrdinalIgnoreCase)
            || improvement.Contains(
                "Review your EPA performance breakdown above for specific areas.",
                StringComparison.OrdinalIgnoreCase
            )
            || overallAttempt.Contains(
                "Detailed coaching unavailable.",
                StringComparison.OrdinalIgnoreCase
            );
    }

    private static string? NormalizeStrength(string? strength)
    {
        if (string.IsNullOrWhiteSpace(strength))
            return null;

        var text = strength.Trim();

        if (TryParseJsonPayload(text, out var normalized))
            return normalized;

        var cleaned = string.Join(
            " ",
            text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line =>
                    !string.Equals(line, "Clinical Strengths", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(line, "Very good", StringComparison.OrdinalIgnoreCase)
                )
        );

        return string.IsNullOrWhiteSpace(cleaned) ? text : CollapseWhitespace(cleaned);
    }

    private static string? NormalizeImprovement(string? improvement)
    {
        if (string.IsNullOrWhiteSpace(improvement))
            return null;

        return CollapseWhitespace(improvement);
    }

    private static bool TryParseJsonPayload(string text, out string normalized)
    {
        normalized = string.Empty;

        var candidate = ExtractJsonCandidate(text);
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(candidate);
            normalized = doc.RootElement.ValueKind switch
            {
                JsonValueKind.String => CollapseWhitespace(
                    doc.RootElement.GetString() ?? string.Empty
                ),
                JsonValueKind.Array => string.Join(
                    " ",
                    doc.RootElement.EnumerateArray()
                        .Select(NormalizeJsonItem)
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                ),
                JsonValueKind.Object => NormalizeJsonObject(doc.RootElement),
                _ => CollapseWhitespace(doc.RootElement.ToString()),
            };

            return !string.IsNullOrWhiteSpace(normalized);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string NormalizeJsonItem(JsonElement item) =>
        item.ValueKind switch
        {
            JsonValueKind.String => item.GetString() ?? string.Empty,
            JsonValueKind.Object => NormalizeJsonObject(item),
            _ => item.ToString(),
        };

    private static string NormalizeJsonObject(JsonElement item)
    {
        var what = ReadString(item, "what");
        var evidence = ReadString(item, "evidence");
        var whyItMattered = ReadString(item, "why_it_mattered_clinically");

        var parts = new[] { what, evidence, whyItMattered }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToList();

        return parts.Count > 0 ? CollapseWhitespace(string.Join(" ", parts)) : item.ToString();
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
            return null;

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.ToString(),
        };
    }

    private static string ExtractJsonCandidate(string text)
    {
        var firstBracket = text.IndexOf('[');
        var firstBrace = text.IndexOf('{');

        var start =
            firstBracket >= 0 && firstBrace >= 0
                ? Math.Min(firstBracket, firstBrace)
                : Math.Max(firstBracket, firstBrace);

        if (start < 0)
            return string.Empty;

        var endBracket = text.LastIndexOf(']');
        var endBrace = text.LastIndexOf('}');
        var end = Math.Max(endBracket, endBrace);

        if (end <= start)
            return string.Empty;

        return text.Substring(start, end - start + 1).Trim();
    }

    private static string CollapseWhitespace(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
