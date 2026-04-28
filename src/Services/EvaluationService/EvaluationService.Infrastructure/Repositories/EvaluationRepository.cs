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

    public async Task<EvaluationResult?> GetByIdAsync(string id)
    {
        return await _db.EvaluationResults
            .Include(x => x.EpaScores)
            .Include(x => x.Warnings)
            .FirstOrDefaultAsync(x => x.ResultId == id);
    }

    public async Task<IEnumerable<EvaluationResult>> GetByUserIdAsync(string userId)
    {
        return await _db.EvaluationResults
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(EvaluationResult result)
    {
        await _db.EvaluationResults.AddAsync(result);
    }

    public Task UpdateAsync(EvaluationResult result)
    {
        _db.EvaluationResults.Update(result);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _db.EvaluationResults.FirstOrDefaultAsync(x => x.ResultId == id);
        if (entity != null)
        {
            _db.EvaluationResults.Remove(entity);
        }
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}