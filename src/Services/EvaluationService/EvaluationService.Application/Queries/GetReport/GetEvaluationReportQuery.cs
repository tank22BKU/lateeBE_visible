using MediatR;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Queries.GetReport;

public record GetEvaluationReportQuery(string ResultId) : IRequest<EvaluationReportDto?>;

public sealed class GetEvaluationReportHandler
    : IRequestHandler<GetEvaluationReportQuery, EvaluationReportDto?>
{
    private readonly IEvaluationRepository _repo;

    public GetEvaluationReportHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<EvaluationReportDto?> Handle(
        GetEvaluationReportQuery query,
        CancellationToken        ct)
    {
        var evaluation = await _repo.GetByIdAsync(query.ResultId);
        if (evaluation == null) return null;

        var session  = await _repo.GetPracticeSessionByIdAsync(evaluation.PracticeSessionId);
        if (session == null) return null;

        var warnings  = await _repo.GetWarningsByPracticeSessionIdAsync(evaluation.PracticeSessionId);
        var feedback  = await _repo.GetPracticeFeedbackBySessionIdAsync(evaluation.PracticeSessionId);
        var epaScoreEntities = await _repo.GetEpaScoresByEvaluationIdAsync(evaluation.Id);

        List<EpaScoreDto> epaScores;
        string?   diagnosisMatchType    = null;
        int       diagnosisModifier     = 0;
        int       timeModifier          = 0;
        int       warningPenalty        = 0;
        List<string> cognitiveAlerts    = [];
        bool      safetyEscalation      = false;
        string?   evaluationTrace       = null;

        if (epaScoreEntities.Count > 0)
        {
            epaScores = epaScoreEntities.Select(e => new EpaScoreDto
            {
                EpaId            = e.EpaId,
                NumericalScore   = e.NumericalScore,
                EntrustmentLevel = e.EntrustmentLevel,
                FeedbackDetail   = e.FeedbackDetail ?? string.Empty,
                EvidenceCited    = e.EvidenceCited,
                FailurePatterns  = e.FailurePatterns,
                SafetyFlags      = e.SafetyFlags
            }).ToList();
        }
        else if (!string.IsNullOrWhiteSpace(evaluation.FeedbackDetail))
        {
            // Fallback: parse JSON blob (legacy data trước migration)
            epaScores = ParseLegacyBlob(evaluation.FeedbackDetail,
                out diagnosisMatchType, out diagnosisModifier, out timeModifier,
                out warningPenalty, out cognitiveAlerts, out safetyEscalation,
                out evaluationTrace);
        }
        else
        {
            epaScores = [];
        }

        return new EvaluationReportDto
        {
            EvaluationId             = evaluation.Id,
            EpaId                    = evaluation.EpaId,
            PracticeSessionId        = evaluation.PracticeSessionId,
            LearnerId                = session.LearnerId,
            PatientId                = session.PatientId,
            ModuleId                 = session.ModuleId       ?? string.Empty,
            DiscussionType           = session.DiscussionType ?? "Message Type",
            FinalDiagnosis           = session.FinalDiagnosis  ?? string.Empty,
            VpConversationLog        = session.VpConversationLog ?? string.Empty,
            AiReasoningLog           = session.AiReasoningLog   ?? string.Empty,
            Score                    = evaluation.Score,
            Duration                 = evaluation.Duration,
            EvaluationTrace          = evaluationTrace,
            EntrustmentLevel         = evaluation.EntrustmentLevel,
            RubricVersion            = evaluation.RubricVersion,
            DiagnosisMatchType       = diagnosisMatchType ?? "UNKNOWN",
            DiagnosisModifier        = diagnosisModifier,
            TimeModifier             = timeModifier,
            WarningPenalty           = warningPenalty,
            SafetyEscalationRequired = safetyEscalation,
            CognitiveAlerts          = cognitiveAlerts,
            EpaScores                = epaScores,
            CreatedAt                = evaluation.CreatedAt,
            Warnings = warnings.Select(x => new WarningDto
            {
                WarningId         = x.Id,
                PracticeSessionId = x.PracticeSessionId,
                LearnerId         = x.LearnerId         ?? string.Empty,
                Label             = x.Label             ?? string.Empty,
                Description       = x.Description       ?? string.Empty,
                CreatedAt         = x.CreatedAt
            }).ToList(),
            PracticeFeedback = feedback == null ? null : new PracticeFeedbackDto
            {
                Id             = feedback.Id,
                OverallAttempt = feedback.OverallAttempt,
                OverallLabel   = feedback.OverallLabel,
                Strength       = feedback.Strength,
                Improvement    = feedback.Improvement,
                CreatedAt      = feedback.CreatedAt
            }
        };
    }

    private static List<EpaScoreDto> ParseLegacyBlob(
        string blob,
        out string?      diagnosisMatchType,
        out int          diagnosisModifier,
        out int          timeModifier,
        out int          warningPenalty,
        out List<string> cognitiveAlerts,
        out bool         safetyEscalation,
        out string?      evaluationTrace)
    {
        diagnosisMatchType = null;
        diagnosisModifier  = 0;
        timeModifier       = 0;
        warningPenalty     = 0;
        cognitiveAlerts    = [];
        safetyEscalation   = false;
        evaluationTrace    = null;

        try
        {
            using var doc  = System.Text.Json.JsonDocument.Parse(blob);
            var root       = doc.RootElement;
            var epaScores  = new List<EpaScoreDto>();

            if (root.TryGetProperty("EpaScores", out var epas))
            {
                epaScores = epas.EnumerateArray().Select(e => new EpaScoreDto
                {
                    EpaId            = e.TryGetProperty("EpaId", out var eid)   ? eid.GetString()  ?? "" : "",
                    NumericalScore   = e.TryGetProperty("NumericalScore", out var sc) ? sc.GetInt32() : 0,
                    EntrustmentLevel = e.TryGetProperty("EntrustmentLevel", out var el) ? el.GetInt32() : 1,
                    FeedbackDetail   = e.TryGetProperty("FeedbackDetail", out var fd)  ? fd.GetString() ?? "" : "",
                    EvidenceCited    = ParseStringList(e, "EvidenceCited"),
                    FailurePatterns  = ParseStringList(e, "FailurePatterns"),
                    SafetyFlags      = ParseStringList(e, "SafetyFlags")
                }).ToList();
            }

            diagnosisMatchType = root.TryGetProperty("DiagnosisMatchType", out var dm) ? dm.GetString() : null;
            diagnosisModifier  = root.TryGetProperty("DiagnosisModifier",  out var dmv) ? dmv.GetInt32() : 0;
            timeModifier       = root.TryGetProperty("TimeModifier",       out var tm)  ? tm.GetInt32()  : 0;
            warningPenalty     = root.TryGetProperty("WarningPenalty",     out var wp)  ? wp.GetInt32()  : 0;
            cognitiveAlerts    = ParseStringList(root, "CognitiveAlerts");
            safetyEscalation   = root.TryGetProperty("SafetyEscalationRequired", out var se) && se.GetBoolean();
            evaluationTrace    = root.TryGetProperty("EvaluationTrace",    out var et)  ? et.GetString() : null;

            return epaScores;
        }
        catch
        {
            evaluationTrace = blob;
            return [];
        }
    }

    private static List<string> ParseStringList(System.Text.Json.JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var p) || p.ValueKind != System.Text.Json.JsonValueKind.Array)
            return [];
        return p.EnumerateArray()
            .Select(e => e.GetString() ?? "")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}