using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Domain.Repositories;

public interface IPracticeSessionRepository
{
    Task<PracticeSessionResult> GetByIdAsync(string id);
    Task<string> AddAsync(PracticeSessionResult entity);
}
