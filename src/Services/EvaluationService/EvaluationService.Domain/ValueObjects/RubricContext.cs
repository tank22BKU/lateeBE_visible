namespace EvaluationService.Domain.ValueObjects;

public sealed record RubricContext(string EccId, string Version, string FullContent, bool IsAvailable)
{
    public static RubricContext Empty(string eccId) => new(
        EccId:       eccId,
        Version:     "unknown",
        FullContent: string.Empty,
        IsAvailable: false
    );
}