namespace EvaluationService.Application.Dtos;

public class EvaluationReportDto
{
    public string          EvaluationId             { get; set; } = default!;
    public string          EpaId                    { get; set; } = default!;
    public string          PracticeSessionId        { get; set; } = default!;
    public string          LearnerId                { get; set; } = default!;
    public string          PatientId                { get; set; } = default!;
    public string          ModuleId                 { get; set; } = default!;
    public string          DiscussionType           { get; set; } = default!;
    public string          FinalDiagnosis           { get; set; } = default!;
    public string          VpConversationLog        { get; set; } = default!;
    public string          AiReasoningLog           { get; set; } = default!;
    public decimal?        Score                    { get; set; }
    public int?            Duration                 { get; set; }
    public string?         EvaluationTrace          { get; set; }
    public int?            EntrustmentLevel         { get; set; }
    public string?         RubricVersion            { get; set; }
    public string          DiagnosisMatchType       { get; set; } = string.Empty;
    public int             DiagnosisModifier        { get; set; }
    public int             TimeModifier             { get; set; }
    public int             WarningPenalty           { get; set; }
    public bool            SafetyEscalationRequired { get; set; }
    public List<string>    CognitiveAlerts          { get; set; } = [];
    public List<EpaScoreDto> EpaScores              { get; set; } = [];
    public DateTime        CreatedAt                { get; set; }
    public List<WarningDto> Warnings               { get; set; } = [];
    public PracticeFeedbackDto? PracticeFeedback   { get; set; }
}

public class PracticeFeedbackDto
{
    public string   Id             { get; set; } = default!;
    public string?  OverallAttempt { get; set; }
    public string?  OverallLabel   { get; set; }
    public string?  Strength       { get; set; }
    public string?  Improvement    { get; set; }
    public DateTime CreatedAt      { get; set; }
}