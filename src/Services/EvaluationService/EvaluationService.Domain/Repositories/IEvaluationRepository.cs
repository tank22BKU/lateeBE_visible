using EvaluationService.Domain.Entities;
namespace EvaluationService.Domain.Repositories;

public interface IEvaluationRepository
{
    Task<EvaluationResult> GetByIdAsync(string id);
    Task<IEnumerable<EvaluationResult>> GetByUserIdAsync(string userId);
    Task AddAsync(EvaluationResult result);
    Task UpdateAsync(EvaluationResult result);
    Task DeleteAsync(string id);
    Task SaveChangesAsync(); 
}