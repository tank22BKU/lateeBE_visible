using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;
using PracticeSessionService.Infrastructure.Persistance;

namespace PracticeSessionService.Infrastructure.Repositories;

public class PracticeSessionRepository : IPracticeSessionRepository
{
    private readonly PracticeSessionDbContext _db;

    public PracticeSessionRepository(PracticeSessionDbContext db)
    {
        _db = db;
    }

    public async Task<PracticeSession?> GetSessionByIdAsync(string id)
    {
        return await _db.PracticeSessions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PracticeSession?> GetLatestSessionAsync(
        string learnerId,
        string patientId,
        IEnumerable<string> statuses
    )
    {
        var statusList = statuses.Distinct().ToList();
        if (statusList.Count == 0)
            return null;

        return await _db
            .PracticeSessions.AsNoTracking()
            .Where(x =>
                x.LearnerId == learnerId
                && x.PatientId == patientId
                && statusList.Contains(x.Status)
            )
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountSessionsAsync(
        string learnerId,
        string patientId,
        IEnumerable<string> statuses
    )
    {
        var statusList = statuses.Distinct().ToList();
        if (statusList.Count == 0)
            return 0;

        return await _db
            .PracticeSessions.AsNoTracking()
            .Where(x =>
                x.LearnerId == learnerId
                && x.PatientId == patientId
                && statusList.Contains(x.Status)
            )
            .CountAsync();
    }

    public async Task<string> AddSessionAsync(PracticeSession entity)
    {
        _db.PracticeSessions.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public Task UpdateSessionAsync(PracticeSession entity)
    {
        _db.PracticeSessions.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<List<Warning>> GetWarningsBySessionIdAsync(string sessionId)
    {
        return await _db
            .Warnings.AsNoTracking()
            .Where(x => x.PracticeSessionId == sessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddWarningsAsync(IEnumerable<Warning> warnings)
    {
        await _db.Warnings.AddRangeAsync(warnings);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
