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
    
    public async Task<PracticeSessionResult?> GetByIdAsync(string id)
    {
        return await _db.EvaluationResults
            .Include(x => x.Warnings)
            .FirstOrDefaultAsync(x => x.ResultId == id);
    }

    public async Task<PracticeSession?> GetSessionByIdAsync(string id)
    {
        return await _db.PracticeSessions
            .Include(x => x.EvaluationResults)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<string> AddAsync(PracticeSessionResult entity)
    {
        _db.EvaluationResults.Add(entity);
        await _db.SaveChangesAsync();
        return entity.ResultId;
    }
    public async Task<string> AddSessionAsync(PracticeSession entity)
    {
        _db.PracticeSessions.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }
}