using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Domain.Services;

public interface IEpaScoreAggregator
{
    AggregatedEvaluationResult Aggregate(GeminiEvaluationOutput aiOutput, EvaluationInput input);
}

public sealed record AggregatedEvaluationResult(
    List<EvaluationEpaScore> EpaScores,
    int RawTotal,
    int DiagnosisModifier,
    string DiagnosisMatchType,
    int TimeModifier,
    int WarningPenalty,
    int FinalScore,
    int OverallEntrustmentLevel,
    List<string> CognitiveAlerts,
    bool SafetyEscalationRequired,
    string EvaluationTrace
);