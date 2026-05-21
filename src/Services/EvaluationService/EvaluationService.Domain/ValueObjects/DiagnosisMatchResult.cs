namespace EvaluationService.Domain.ValueObjects;

public sealed class DiagnosisMatchResult
{
    public static readonly IReadOnlySet<string> ValidMatchTypes = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "EXACT_MATCH",
        "SEMANTIC_MATCH",
        "PARTIAL_MATCH",
        "WRONG",
        "DANGEROUS",
        "NO_DIAGNOSIS",
        "UNKNOWN",
        "UNVERIFIED",
        "MATCH",
        "PARTIAL",
    };

    public string MatchType { get; }

    public bool IsAcceptable =>
        MatchType is "EXACT_MATCH" or "SEMANTIC_MATCH" or "PARTIAL_MATCH" or "MATCH" or "PARTIAL";

    public bool IsDangerous => MatchType is "DANGEROUS";
    public bool RequiresSafetyReview => IsDangerous || MatchType is "NO_DIAGNOSIS";

    public string MatchTypeLabel =>
        MatchType switch
        {
            "EXACT_MATCH" => "Exact match",
            "SEMANTIC_MATCH" => "Equivalent match",
            "PARTIAL_MATCH" => "Partial match",
            "MATCH" => "Exact match",
            "PARTIAL" => "Partial match",
            "WRONG" => "Incorrect",
            "DANGEROUS" => "Dangerous error",
            "NO_DIAGNOSIS" => "Not submitted",
            "UNKNOWN" => "Unverified",
            "UNVERIFIED" => "Unverified",
            _ => "Unknown",
        };

    private DiagnosisMatchResult(string matchType) => MatchType = matchType;

    public static DiagnosisMatchResult From(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new DiagnosisMatchResult("UNKNOWN");
        var normalized = raw.Trim().ToUpperInvariant();
        return ValidMatchTypes.Contains(normalized)
            ? new DiagnosisMatchResult(normalized)
            : new DiagnosisMatchResult("UNKNOWN");
    }
}
