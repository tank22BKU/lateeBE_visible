using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Domain.Services;

/// Load và cache rubric content. Tách khỏi AI layer.
public interface IRubricProvider
{
    Task<RubricContext> GetRubricAsync(string eccId, CancellationToken ct = default);
}
