namespace EvaluationService.Application.Dtos;

public class ScoringAdjustmentDto
{
    public string Code { get; set; } = string.Empty; // "DIAGNOSIS_EXACT_MATCH"
    public string Title { get; set; } = string.Empty; // "Exact diagnosis match"
    public int Score { get; set; } // +10 hoặc -3
    public string Reason { get; set; } = string.Empty; // giải thích dynamic
    public string Source { get; set; } = string.Empty; // "diagnosis" | "time" | "warning"
    public string Severity { get; set; } = string.Empty; // "positive" | "low" | "medium" | "high" | "critical"
}

public class ScoringAdjustmentsDto
{
    public List<ScoringAdjustmentDto> Positive { get; set; } = [];
    public List<ScoringAdjustmentDto> Negative { get; set; } = [];
    public ValidationSummaryDto Validation { get; set; } = new();
}

public class ValidationSummaryDto
{
    public bool HasEthicsViolation { get; set; }
    public bool HasUnsafeQuestion { get; set; }
    public bool HasWorkflowViolation { get; set; }
    public bool SafetyEscalationRequired { get; set; }
    public int TotalWarnings { get; set; }
}

public class DiagnosisMatchDto
{
    public string MatchType { get; set; } = string.Empty; // "EXACT_MATCH"
    public string MatchTypeLabel { get; set; } = string.Empty; // "Exact match"
    public bool IsAcceptable { get; set; }
    public bool IsDangerous { get; set; }
    public bool RequiresSafetyReview { get; set; }
}
