using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Services;

/// Nhận AI raw output → validate bounds → apply ScoringModifiers → return AggregatedResult.
/// Đây là "sanity check" layer giữa AI output và persistence.
public sealed class EpaScoreAggregator : IEpaScoreAggregator
{
    private readonly ILogger<EpaScoreAggregator> _logger;

    public EpaScoreAggregator(ILogger<EpaScoreAggregator> logger)
        => _logger = logger;

    public AggregatedEvaluationResult Aggregate(
        GeminiEvaluationOutput aiOutput,
        EvaluationInput        input)
    {
        var epaScoreEntities = aiOutput.EpaScores
            .Take(5)
            .Select(s => new EvaluationEpaScore
            {
                Id               = Guid.NewGuid().ToString("N"),
                EpaId            = s.EpaId,
                NumericalScore   = Math.Clamp(s.NumericalScore, 0, 20),
                EntrustmentLevel = Math.Clamp(s.EntrustmentLevel, 1, 5),
                FeedbackDetail   = s.FeedbackDetail,
                EvidenceCited    = s.EvidenceCited   ?? [],
                FailurePatterns  = s.FailurePatterns  ?? [],
                SafetyFlags      = s.SafetyFlags      ?? [],
                CreatedAt        = DateTime.UtcNow
            })
            .ToList();

        var rawTotal = epaScoreEntities.Sum(x => x.NumericalScore);

        var allottedTotal = input.AllottedVpTimeMinutes + input.AllottedArgumentTimeMinutes;
        var scoring = ScoringModifiers.Calculate(
            rawTotal:             rawTotal,
            diagnosisMatchType:   aiOutput.DiagnosisMatchType,
            actualDurationMinutes: input.ActualDurationMinutes,
            allottedTotalMinutes: allottedTotal,
            warningLabels:        input.ActiveWarningLabels
        );

        var diff = Math.Abs(aiOutput.FinalScore - scoring.FinalScore);
        if (diff > 5)
        {
            _logger.LogWarning(
                "Score discrepancy: AI reported {AiScore}, backend computed {BackendScore} (diff={Diff})",
                aiOutput.FinalScore, scoring.FinalScore, diff);
        }

        return new AggregatedEvaluationResult(
            EpaScores:               epaScoreEntities,
            RawTotal:                rawTotal,
            DiagnosisModifier:       scoring.DiagnosisModifier,
            DiagnosisMatchType:      aiOutput.DiagnosisMatchType,
            TimeModifier:            scoring.TimeModifier,
            WarningPenalty:          scoring.WarningPenalty,
            FinalScore:              scoring.FinalScore,           
            OverallEntrustmentLevel: scoring.EntrustmentLevel,
            CognitiveAlerts:         aiOutput.CognitiveAlerts,
            SafetyEscalationRequired: scoring.SafetyEscalationRequired,
            EvaluationTrace:         aiOutput.EvaluationTrace
        );
    }
}