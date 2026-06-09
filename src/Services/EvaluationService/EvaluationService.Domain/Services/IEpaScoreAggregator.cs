using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Domain.Services;

public interface IEpaScoreAggregator
{
    AggregatedEvaluationResult Aggregate(GeminiEvaluationOutput aiOutput, EvaluationInput input);
}

/// finalScore = CLAMP(pureEpaScore + adjustments.AdjustmentTotal, 0, 110)
public sealed record AggregatedEvaluationResult(
    List<EvaluationEpaScore> EpaScores,
    int PureEpaScore,
    int PositiveAdjustmentTotal,
    int NegativeAdjustmentTotal,
    int AdjustmentTotal,
    string DiagnosisMatchType,
    int FinalScore,
    int OverallEntrustmentLevel,
    List<string> CognitiveAlerts,
    bool SafetyEscalationRequired,
    string EvaluationTrace,
    ScoringAdjustments Adjustments
);
