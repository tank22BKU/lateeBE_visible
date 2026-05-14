namespace EvaluationService.Application.Dtos;

public class SubmitEvaluationResultDto
{
    public string            EvaluationId             { get; set; } = string.Empty;
    public string            PracticeSessionId        { get; set; } = string.Empty;
    public decimal?          Score                    { get; set; }
    public int?              EntrustmentLevel         { get; set; }
    public string?           FeedbackDetail           { get; set; } 
    public string            FinalDiagnosis           { get; set; } = string.Empty;
    public string            DiagnosisMatchType       { get; set; } = string.Empty;
    public int               DiagnosisModifier        { get; set; }
    public int               TimeModifier             { get; set; }
    public int               WarningPenalty           { get; set; }
    public int               WarningCount             { get; set; }
    public bool              SafetyEscalationRequired { get; set; }
    public List<string>      CognitiveAlerts          { get; set; } = [];
    public List<EpaScoreDto> EpaScores                { get; set; } = [];
    public string            DiscussionType           { get; set; } = string.Empty;
    public int?              Duration                 { get; set; }
    public bool              PracticeFeedbackAvailable { get; set; }
}