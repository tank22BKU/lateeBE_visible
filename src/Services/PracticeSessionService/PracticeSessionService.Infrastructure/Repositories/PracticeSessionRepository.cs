using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Repositories;

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
        return await _db.PracticeSessions
            .FirstOrDefaultAsync(x => x.Id == id);
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
        return await _db.Warnings
            .AsNoTracking()
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