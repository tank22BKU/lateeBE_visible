using EvaluationService.Domain.Entities;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Domain.Repositories;

public interface IAiEvaluationProvider
{
    Task<GeminiEvaluationOutput> AnalyzePerformanceAsync(
        string prompt,
        CancellationToken ct = default
    );
}

public sealed record EvaluationInput(
    string SessionId,
    string LearnerId,
    string PatientId,
    string VpConversationLog,
    string AiReasoningLog,
    string LearnerFinalDiagnosis,
    string CanonicalDiagnosis,
    string CaseDescription,
    int AllottedVpTimeMinutes,
    int AllottedArgumentTimeMinutes,
    int ActualDurationMinutes,
    string RubricContent,
    string RubricVersion,
    List<string> ActiveWarningLabels,
    List<string> ActiveWarningDescriptions
);

public sealed record GeminiEvaluationOutput(
    List<EvaluationEpaScore> EpaScores,
    int DiagnosisModifier,
    string DiagnosisMatchType,
    int TimeModifier,
    int TotalWarningPenalty,
    int FinalScore,
    int OverallEntrustmentLevel,
    List<string> CognitiveAlerts,
    bool SafetyEscalationRequired,
    string EvaluationTrace,
    AdjustmentExplanation? AdjustmentExplanations
);

public sealed record AdjustmentExplanation(
    string? Diagnosis,
    string? Time,
    List<WarningExplanation> Warnings
);

public sealed record WarningExplanation(string Label, string Reason);
