using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;

namespace EvaluationService.Application.Services;

public interface IEvaluationPersistenceService
{
    Task<string> PersistEvaluationAsync(
        string                   practiceSessionId,
        AggregatedEvaluationResult result,
        string                   rubricVersion,
        int                      duration,
        CancellationToken        ct = default);
}

public sealed class EvaluationPersistenceService : IEvaluationPersistenceService
{
    private readonly IEvaluationRepository _repo;

    public EvaluationPersistenceService(IEvaluationRepository repo) => _repo = repo;

    public async Task<string> PersistEvaluationAsync(
        string                   practiceSessionId,
        AggregatedEvaluationResult result,
        string                   rubricVersion,
        int                      duration,
        CancellationToken        ct = default)
    {
        var evaluationId = Guid.NewGuid().ToString("N");
        var evaluation = new Evaluation
        {
            Id                = evaluationId,
            EpaId             = "EPA_COMPOSITE",
            PracticeSessionId = practiceSessionId,
            Score             = result.FinalScore,
            Duration          = duration,
            CreatedAt         = DateTime.UtcNow,
            EntrustmentLevel  = result.OverallEntrustmentLevel,
            RubricVersion     = rubricVersion,
        };

        await _repo.AddEvaluationAsync(evaluation);

        foreach (var epa in result.EpaScores)
            epa.EvaluationId = evaluationId;

        await _repo.AddEpaScoresAsync(result.EpaScores);

        await _repo.SaveChangesAsync();

        return evaluationId;
    }

    private static string BuildLegacyJson(AggregatedEvaluationResult result)
    {
        var obj = new
        {
            EpaScores = result.EpaScores.Select(s => new
            {
                s.EpaId,
                NumericalScore   = s.NumericalScore,
                EntrustmentLevel = s.EntrustmentLevel,
                FeedbackDetail   = s.FeedbackDetail,
                EvidenceCited    = s.EvidenceCited,
                FailurePatterns  = s.FailurePatterns,
                SafetyFlags      = s.SafetyFlags
            }),
            DiagnosisMatchType       = result.DiagnosisMatchType,
            DiagnosisModifier        = result.DiagnosisModifier,
            TimeModifier             = result.TimeModifier,
            WarningPenalty           = result.WarningPenalty,
            CognitiveAlerts          = result.CognitiveAlerts,
            SafetyEscalationRequired = result.SafetyEscalationRequired,
            EvaluationTrace          = result.EvaluationTrace
        };
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
}