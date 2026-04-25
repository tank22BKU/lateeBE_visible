using EvaluationService.Domain.Entities;

namespace EvaluationService.Domain.Repositories;

public interface IGeminiAiRepository
{
    Task<List<EpaScore>> AnalyzePerformanceAsync(EvaluationResult result);
}