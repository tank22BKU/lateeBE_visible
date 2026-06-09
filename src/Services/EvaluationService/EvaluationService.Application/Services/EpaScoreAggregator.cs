using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Application.Services;

public sealed class EpaScoreAggregator : IEpaScoreAggregator
{
    private readonly ILogger<EpaScoreAggregator> _logger;

    public EpaScoreAggregator(ILogger<EpaScoreAggregator> logger) => _logger = logger;

    public AggregatedEvaluationResult Aggregate(
        GeminiEvaluationOutput aiOutput,
        EvaluationInput input
    )
    {
        var epaScores = aiOutput
            .EpaScores.Take(5)
            .Select(s => new EvaluationEpaScore
            {
                Id = Guid.NewGuid().ToString("N"),
                EpaId = s.EpaId,
                NumericalScore = Math.Clamp(s.NumericalScore, 0, 20),
                EntrustmentLevel = Math.Clamp(s.EntrustmentLevel, 1, 5),
                FeedbackDetail = s.FeedbackDetail,
                EvidenceCited = s.EvidenceCited ?? [],
                FailurePatterns = s.FailurePatterns ?? [],
                SafetyFlags = s.SafetyFlags ?? [],
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        var pureEpaScore = epaScores.Sum(x => x.NumericalScore);

        var allottedTotal = input.AllottedVpTimeMinutes + input.AllottedArgumentTimeMinutes;

        var adjustments = AdjustmentRuleEngine.Calculate(
            diagnosisMatchType: aiOutput.DiagnosisMatchType,
            learnerDiagnosis: input.LearnerFinalDiagnosis,
            canonicalDiagnosis: input.CanonicalDiagnosis,
            actualDurationMinutes: input.ActualDurationMinutes,
            allottedTotalMinutes: allottedTotal,
            warningLabels: input.ActiveWarningLabels,
            warningDescriptions: input.ActiveWarningDescriptions,
            pureEpaScore: pureEpaScore,
            explanation: aiOutput.AdjustmentExplanations
        );

        var finalScore = AdjustmentRuleEngine.ComputeFinalScore(pureEpaScore, adjustments);
        var level = AdjustmentRuleEngine.MapEntrustmentLevel(finalScore);

        var diff = Math.Abs(aiOutput.FinalScore - finalScore);
        if (diff > 5)
            _logger.LogWarning(
                "Score discrepancy: AI={AiScore} backend={BackendScore} diff={Diff} "
                    + "pureEpa={PureEpa} adjTotal={AdjTotal}",
                aiOutput.FinalScore,
                finalScore,
                diff,
                pureEpaScore,
                adjustments.AdjustmentTotal
            );

        return new AggregatedEvaluationResult(
            EpaScores: epaScores,
            PureEpaScore: pureEpaScore,
            PositiveAdjustmentTotal: adjustments.PositiveTotal,
            NegativeAdjustmentTotal: adjustments.NegativeTotal,
            AdjustmentTotal: adjustments.AdjustmentTotal,
            DiagnosisMatchType: aiOutput.DiagnosisMatchType,
            FinalScore: finalScore,
            OverallEntrustmentLevel: level,
            CognitiveAlerts: aiOutput.CognitiveAlerts,
            SafetyEscalationRequired: adjustments.Validation.SafetyEscalationRequired,
            EvaluationTrace: aiOutput.EvaluationTrace,
            Adjustments: adjustments
        );
    }
}
