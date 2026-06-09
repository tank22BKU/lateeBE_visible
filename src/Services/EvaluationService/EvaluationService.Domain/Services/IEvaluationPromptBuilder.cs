using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Domain.Services;

/// Prompt AI evaluator từ input + rubric context.
public interface IEvaluationPromptBuilder
{
    string Build(EvaluationInput input, RubricContext rubric);
}
