using EvaluationService.Domain.Entities;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Domain.Repositories;

public interface IAiEvaluationProvider
{
    Task<GeminiEvaluationOutput> AnalyzePerformanceAsync(
        string prompt,
        CancellationToken ct = default);
}

public sealed record EvaluationInput(
    string SessionId,
    string LearnerId,
    string PatientId,
    string VpConversationLog,
    string AiReasoningLog,
    string LearnerFinalDiagnosis,
    string CanonicalDiagnosis,        //  clinical_case.type
    string CaseDescription,           //  clinical_case.description
    int AllottedVpTimeMinutes,        //  virtual_patient.time_setting
    int AllottedArgumentTimeMinutes,  //  virtual_patient.argument_time
    int ActualDurationMinutes,        //  computed from start_time/end_time
    string RubricContent,             //  evaluation_clinical_criteria.description
    string RubricVersion,
    List<string> ActiveWarningLabels
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
    string EvaluationTrace
);