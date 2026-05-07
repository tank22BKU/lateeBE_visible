using EvaluationService.Domain.Entities;
namespace EvaluationService.Domain.Repositories;

public interface IEvaluationRepository
{
    Task<Evaluation?> GetByIdAsync(string id);
    Task<PracticeSession?> GetPracticeSessionByIdAsync(string id);
    Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(string practiceSessionId);
    Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId);
    Task AddEvaluationAsync(Evaluation evaluation);
    Task AddWarningsAsync(IEnumerable<Warning> warnings);
    Task UpdatePracticeSessionAsync(PracticeSession session);
    Task DeleteAsync(string id);
    Task SaveChangesAsync();
}