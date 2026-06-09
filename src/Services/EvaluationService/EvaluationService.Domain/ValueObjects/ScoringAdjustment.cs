namespace EvaluationService.Domain.ValueObjects;

/// score > 0 = bonus, score < 0 = penalty.
public sealed record ScoringAdjustment(
    string Code, // "DIAGNOSIS_EXACT_MATCH"
    string Title, // "Exact diagnosis match"
    int Score, // +10, -3
    string Reason, // dynamic: "Learner submitted 'Acute appendicitis', matched canonical diagnosis exactly."
    string Source, // "diagnosis" | "time" | "warning"
    string Severity // "positive" | "low" | "medium" | "high" | "critical"
);

public sealed class ScoringAdjustments
{
    public IReadOnlyList<ScoringAdjustment> Positive { get; }
    public IReadOnlyList<ScoringAdjustment> Negative { get; }
    public ValidationSummary Validation { get; }

    public int PositiveTotal => Positive.Sum(a => a.Score);
    public int NegativeTotal => Negative.Sum(a => Math.Abs(a.Score));
    public int AdjustmentTotal => PositiveTotal - NegativeTotal;

    public ScoringAdjustments(
        IReadOnlyList<ScoringAdjustment> positive,
        IReadOnlyList<ScoringAdjustment> negative,
        ValidationSummary validation
    )
    {
        Positive = positive;
        Negative = negative;
        Validation = validation;
    }
}

public sealed record ValidationSummary(
    bool HasEthicsViolation,
    bool HasUnsafeQuestion,
    bool HasWorkflowViolation,
    bool SafetyEscalationRequired,
    int TotalWarnings
);
