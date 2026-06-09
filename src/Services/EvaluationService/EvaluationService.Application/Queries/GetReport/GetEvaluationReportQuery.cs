using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.ValueObjects;
using MediatR;

namespace EvaluationService.Application.Queries.GetReport;

public record GetEvaluationReportQuery(string ResultId) : IRequest<EvaluationReportDto?>;

public sealed class GetEvaluationReportHandler
    : IRequestHandler<GetEvaluationReportQuery, EvaluationReportDto?>
{
    private readonly IEvaluationRepository _repo;

    public GetEvaluationReportHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<EvaluationReportDto?> Handle(
        GetEvaluationReportQuery query,
        CancellationToken ct
    )
    {
        var evaluation = await _repo.GetByIdAsync(query.ResultId);
        if (evaluation == null)
            return null;

        var session = await _repo.GetPracticeSessionByIdAsync(evaluation.PracticeSessionId);
        if (session == null)
            return null;

        var warnings = await _repo.GetWarningsByPracticeSessionIdAsync(
            evaluation.PracticeSessionId
        );
        var feedback = await _repo.GetPracticeFeedbackBySessionIdAsync(
            evaluation.PracticeSessionId
        );
        var epaScoreEntities = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);

        List<EpaScoreDto> epaScores;
        string? diagnosisMatchType = null;
        int diagnosisModifier = 0;
        int timeModifier = 0;
        int warningPenalty = 0;
        List<string> cognitiveAlerts = [];
        bool safetyEscalation = false;
        string? evaluationTrace = null;
        int pureEpaScore = 0;
        int positiveAdjTotal = 0;
        int negativeAdjTotal = 0;
        int adjustmentTotal = 0;
        ScoringAdjustmentsDto? adjustmentsDto = null;

        if (epaScoreEntities.Count > 0)
        {
            epaScores = epaScoreEntities.Select(MapEpaDto).ToList();
            pureEpaScore =
                evaluation.PureEpaScore > 0
                    ? evaluation.PureEpaScore
                    : epaScoreEntities.Sum(e => e.NumericalScore);

            if (!string.IsNullOrWhiteSpace(evaluation.FeedbackDetail))
                ParseLegacyBlob(
                    evaluation.FeedbackDetail,
                    out diagnosisMatchType,
                    out diagnosisModifier,
                    out timeModifier,
                    out warningPenalty,
                    out cognitiveAlerts,
                    out safetyEscalation,
                    out evaluationTrace,
                    out positiveAdjTotal,
                    out negativeAdjTotal,
                    out adjustmentTotal,
                    out adjustmentsDto
                );
        }
        else if (!string.IsNullOrWhiteSpace(evaluation.FeedbackDetail))
        {
            epaScores = ParseLegacyBlob(
                evaluation.FeedbackDetail,
                out diagnosisMatchType,
                out diagnosisModifier,
                out timeModifier,
                out warningPenalty,
                out cognitiveAlerts,
                out safetyEscalation,
                out evaluationTrace,
                out positiveAdjTotal,
                out negativeAdjTotal,
                out adjustmentTotal,
                out adjustmentsDto
            );

            pureEpaScore =
                evaluation.PureEpaScore > 0
                    ? evaluation.PureEpaScore
                    : epaScores.Sum(e => e.NumericalScore);
        }
        else
        {
            epaScores = [];
        }

        var diagResult = DiagnosisMatchResult.From(diagnosisMatchType);

        return new EvaluationReportDto
        {
            EvaluationId = evaluation.Id,
            EpaId = evaluation.EpaId,
            PracticeSessionId = evaluation.PracticeSessionId,
            LearnerId = session.LearnerId,
            PatientId = session.PatientId,
            ModuleId = session.ModuleId ?? string.Empty,
            DiscussionType = session.DiscussionType ?? "Message Type",
            FinalDiagnosis = session.FinalDiagnosis ?? string.Empty,
            VpConversationLog = session.VpConversationLog ?? string.Empty,
            AiReasoningLog = session.AiReasoningLog ?? string.Empty,
            Score = evaluation.Score,
            Duration = evaluation.Duration,
            EvaluationTrace = evaluationTrace,
            EntrustmentLevel = evaluation.EntrustmentLevel,
            RubricVersion = evaluation.RubricVersion,

            PureEpaScore = pureEpaScore,
            PositiveAdjustmentTotal = positiveAdjTotal,
            NegativeAdjustmentTotal = negativeAdjTotal,
            AdjustmentTotal = adjustmentTotal,

            DiagnosisMatch = new DiagnosisMatchDto
            {
                MatchType = diagResult.MatchType,
                MatchTypeLabel = diagResult.MatchTypeLabel,
                IsAcceptable = diagResult.IsAcceptable,
                IsDangerous = diagResult.IsDangerous,
                RequiresSafetyReview = diagResult.RequiresSafetyReview,
            },

            DiagnosisMatchType = diagnosisMatchType ?? "UNKNOWN",
            DiagnosisModifier = diagnosisModifier,
            TimeModifier = timeModifier,
            WarningPenalty = warningPenalty,
            SafetyEscalationRequired = safetyEscalation,
            CognitiveAlerts = cognitiveAlerts,

            EpaScores = epaScores,
            Adjustments = adjustmentsDto ?? new ScoringAdjustmentsDto(),
            CreatedAt = evaluation.CreatedAt,

            Warnings = warnings
                .Select(x => new WarningDto
                {
                    WarningId = x.Id,
                    PracticeSessionId = x.PracticeSessionId,
                    LearnerId = x.LearnerId ?? string.Empty,
                    Label = x.Label ?? string.Empty,
                    Description = x.Description ?? string.Empty,
                    CreatedAt = x.CreatedAt,
                })
                .ToList(),

            PracticeFeedback =
                feedback == null
                    ? null
                    : new PracticeFeedbackDto
                    {
                        Id = feedback.Id,
                        OverallAttempt = feedback.OverallAttempt,
                        OverallLabel = feedback.OverallLabel,
                        Strength = feedback.Strength,
                        Improvement = feedback.Improvement,
                        CreatedAt = feedback.CreatedAt,
                    },
        };
    }

    private static EpaScoreDto MapEpaDto(Domain.Entities.EvaluationEpaScore e) =>
        new()
        {
            EpaId = e.EpaId,
            NumericalScore = e.NumericalScore,
            MaxScore = 20,
            EntrustmentLevel = e.EntrustmentLevel,
            FeedbackDetail = e.FeedbackDetail ?? string.Empty,
            EvidenceCited = e.EvidenceCited,
            FailurePatterns = e.FailurePatterns,
            SafetyFlags = e.SafetyFlags,
        };

    private static List<EpaScoreDto> ParseLegacyBlob(
        string blob,
        out string? diagnosisMatchType,
        out int diagnosisModifier,
        out int timeModifier,
        out int warningPenalty,
        out List<string> cognitiveAlerts,
        out bool safetyEscalation,
        out string? evaluationTrace,
        out int positiveAdjTotal,
        out int negativeAdjTotal,
        out int adjustmentTotal,
        out ScoringAdjustmentsDto? adjustmentsDto
    )
    {
        diagnosisMatchType = null;
        diagnosisModifier = 0;
        timeModifier = 0;
        warningPenalty = 0;
        cognitiveAlerts = [];
        safetyEscalation = false;
        evaluationTrace = null;
        positiveAdjTotal = 0;
        negativeAdjTotal = 0;
        adjustmentTotal = 0;
        adjustmentsDto = null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(blob);
            var root = doc.RootElement;

            var epaScores = new List<EpaScoreDto>();
            if (root.TryGetProperty("EpaScores", out var epas))
            {
                epaScores = epas.EnumerateArray()
                    .Select(e => new EpaScoreDto
                    {
                        EpaId = GetStr(e, "EpaId"),
                        NumericalScore = GetInt(e, "NumericalScore"),
                        MaxScore = 20,
                        EntrustmentLevel = GetInt(e, "EntrustmentLevel", 1),
                        FeedbackDetail = GetStr(e, "FeedbackDetail"),
                        EvidenceCited = ParseStringList(e, "EvidenceCited"),
                        FailurePatterns = ParseStringList(e, "FailurePatterns"),
                        SafetyFlags = ParseStringList(e, "SafetyFlags"),
                    })
                    .ToList();
            }

            diagnosisMatchType = GetStrNullable(root, "DiagnosisMatchType");
            diagnosisModifier = GetInt(root, "DiagnosisModifier");
            timeModifier = GetInt(root, "TimeModifier");
            warningPenalty = GetInt(root, "WarningPenalty");
            cognitiveAlerts = ParseStringList(root, "CognitiveAlerts");
            safetyEscalation =
                root.TryGetProperty("SafetyEscalationRequired", out var se) && se.GetBoolean();
            evaluationTrace = GetStrNullable(root, "EvaluationTrace");
            positiveAdjTotal = GetInt(root, "PositiveAdjustmentTotal");
            negativeAdjTotal = GetInt(root, "NegativeAdjustmentTotal");
            adjustmentTotal = GetInt(root, "AdjustmentTotal");

            if (root.TryGetProperty("Adjustments", out var adjEl))
                adjustmentsDto = ParseAdjustmentsDto(adjEl, safetyEscalation);

            return epaScores;
        }
        catch
        {
            evaluationTrace = blob;
            return [];
        }
    }

    private static ScoringAdjustmentsDto ParseAdjustmentsDto(
        System.Text.Json.JsonElement root,
        bool safetyEscalation
    ) =>
        new()
        {
            Positive = ParseAdjList(root, "Positive"),
            Negative = ParseAdjList(root, "Negative"),
            Validation = new ValidationSummaryDto { SafetyEscalationRequired = safetyEscalation },
        };

    private static List<ScoringAdjustmentDto> ParseAdjList(
        System.Text.Json.JsonElement root,
        string prop
    )
    {
        if (
            !root.TryGetProperty(prop, out var arr)
            || arr.ValueKind != System.Text.Json.JsonValueKind.Array
        )
            return [];

        return arr.EnumerateArray()
            .Select(e => new ScoringAdjustmentDto
            {
                Code = GetStr(e, "Code"),
                Title = GetStr(e, "Title"),
                Score = GetInt(e, "Score"),
                Reason = GetStr(e, "Reason"),
                Source = GetStr(e, "Source"),
                Severity = GetStr(e, "Severity"),
            })
            .ToList();
    }

    private static string GetStr(System.Text.Json.JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) ? v.GetString() ?? "" : "";

    private static string? GetStrNullable(System.Text.Json.JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) ? v.GetString() : null;

    private static int GetInt(System.Text.Json.JsonElement e, string p, int def = 0) =>
        e.TryGetProperty(p, out var v) && v.TryGetInt32(out var i) ? i : def;

    private static List<string> ParseStringList(System.Text.Json.JsonElement el, string prop)
    {
        if (
            !el.TryGetProperty(prop, out var p)
            || p.ValueKind != System.Text.Json.JsonValueKind.Array
        )
            return [];
        return p.EnumerateArray()
            .Select(e => e.GetString() ?? "")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}
