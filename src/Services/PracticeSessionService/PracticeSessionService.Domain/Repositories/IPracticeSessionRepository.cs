using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Domain.Repositories;

public interface IPracticeSessionRepository
{
    Task<PracticeSession?> GetSessionByIdAsync(string id);
    Task<PracticeSession?> GetLatestSessionAsync(
        string learnerId,
        string patientId,
        IEnumerable<string> statuses);
    Task<int> CountSessionsAsync(
        string learnerId,
        string patientId,
        IEnumerable<string> statuses);
    Task<string> AddSessionAsync(PracticeSession entity);
    Task UpdateSessionAsync(PracticeSession entity);
    Task<List<Warning>> GetWarningsBySessionIdAsync(string sessionId);
    Task AddWarningsAsync(IEnumerable<Warning> warnings);
    Task SaveChangesAsync();
}
