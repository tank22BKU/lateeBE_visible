using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EvaluationService.Infrastructure.Repositories;

public class EvaluationRepository : IEvaluationRepository
{
    private readonly EvaluationDbContext _db;

    public EvaluationRepository(EvaluationDbContext db)
    {
        _db = db;
    }

    public async Task<Evaluation?> GetByIdAsync(string id)
    {
        return await _db.Evaluations.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PracticeSession?> GetPracticeSessionByIdAsync(string id)
    {
        return await _db.PracticeSessions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(string practiceSessionId)
    {
        return await _db.Warnings
            .AsNoTracking()
            .Where(x => x.PracticeSessionId == practiceSessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId)
    {
        return await _db.Evaluations
            .Join(_db.PracticeSessions,
                eval => eval.PracticeSessionId,
                session => session.Id,
                (eval, session) => new { eval, session })
            .Where(x => x.session.LearnerId == learnerId)
            .OrderByDescending(x => x.eval.CreatedAt)
            .Select(x => x.eval)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddEvaluationAsync(Evaluation evaluation)
    {
        await _db.Evaluations.AddAsync(evaluation);
    }

    public async Task AddWarningsAsync(IEnumerable<Warning> warnings)
    {
        await _db.Warnings.AddRangeAsync(warnings);
    }

    public Task UpdatePracticeSessionAsync(PracticeSession session)
    {
        _db.PracticeSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _db.Evaluations.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            _db.Evaluations.Remove(entity);
        }
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
    
}