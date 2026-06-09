using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;

namespace EvaluationService.Application.Services;

public interface IEvaluationPersistenceService
{
    Task<string> PersistEvaluationAsync(
        string practiceSessionId,
        AggregatedEvaluationResult result,
        string rubricVersion,
        int duration,
        CancellationToken ct = default
    );
}

public sealed class EvaluationPersistenceService : IEvaluationPersistenceService
{
    private readonly IEvaluationRepository _repo;

    public EvaluationPersistenceService(IEvaluationRepository repo) => _repo = repo;

    public async Task<string> PersistEvaluationAsync(
        string practiceSessionId,
        AggregatedEvaluationResult result,
        string rubricVersion,
        int duration,
        CancellationToken ct = default
    )
    {
        var evaluationId = Guid.NewGuid().ToString("N");

        var evaluation = new Evaluation
        {
            Id = evaluationId,
            EpaId = "EPA_COMPOSITE",
            PracticeSessionId = practiceSessionId,
            Score = result.FinalScore,
            Duration = duration,
            CreatedAt = DateTime.UtcNow,
            EntrustmentLevel = result.OverallEntrustmentLevel,
            RubricVersion = rubricVersion,
            PureEpaScore = result.PureEpaScore,
            FeedbackDetail = BuildLegacyJson(result),
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
        var diagAdj = result
            .Adjustments.Positive.Concat(result.Adjustments.Negative)
            .FirstOrDefault(a => a.Source == "diagnosis");
        var timeAdj = result
            .Adjustments.Positive.Concat(result.Adjustments.Negative)
            .FirstOrDefault(a => a.Source == "time");

        var obj = new
        {
            EpaScores = result.EpaScores.Select(s => new
            {
                s.EpaId,
                s.NumericalScore,
                s.EntrustmentLevel,
                s.FeedbackDetail,
                s.EvidenceCited,
                s.FailurePatterns,
                s.SafetyFlags,
            }),
            PureEpaScore = result.PureEpaScore,
            PositiveAdjustmentTotal = result.PositiveAdjustmentTotal,
            NegativeAdjustmentTotal = result.NegativeAdjustmentTotal,
            AdjustmentTotal = result.AdjustmentTotal,
            DiagnosisMatchType = result.DiagnosisMatchType,
            DiagnosisModifier = diagAdj?.Score ?? 0,
            TimeModifier = timeAdj?.Score ?? 0,
            WarningPenalty = result
                .Adjustments.Negative.Where(a => a.Source == "warning")
                .Sum(a => Math.Abs(a.Score)),
            CognitiveAlerts = result.CognitiveAlerts,
            SafetyEscalationRequired = result.SafetyEscalationRequired,
            EvaluationTrace = result.EvaluationTrace,
            Adjustments = new
            {
                Positive = result.Adjustments.Positive,
                Negative = result.Adjustments.Negative,
                Validation = result.Adjustments.Validation,
            },
        };

        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
}
