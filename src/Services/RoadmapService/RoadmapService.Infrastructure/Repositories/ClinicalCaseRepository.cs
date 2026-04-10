using RoadmapService.Domain.Entities;
using RoadmapService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Infrastructure.Repositories;

public class ClinicalCaseRepository : IClinicalCaseRepository
{
    private readonly ClinicalCaseDbContext _db;

    public ClinicalCaseRepository(ClinicalCaseDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClinicalCase>> GetAllAsync()
    {
        return await _db.ClinicalCases.ToListAsync();
    }

    public async Task<List<ClinicalCase>> GetFirstAsync()
    {
        return await _db.ClinicalCases.ToListAsync();
    }

    public Task<ClinicalCase?> GetByIdAsync(string id)
    {
        return _db.ClinicalCases
            .FirstOrDefaultAsync(x => x.ClinicalCaseId == id);
    }

    public Task<List<ClinicalCase>> GetActiveAsync(int limit)
    {
        return _db.ClinicalCases
            .Where(x => x.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<(List<ClinicalCase> Items, int Total)>
    GetPagedAsync(string? status, int page, int pageSize)
    {
        var ClinicalcasesQuery = _db.ClinicalCases.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
        {
            ClinicalcasesQuery = ClinicalcasesQuery.Where(x => x.Status == status);
        }

        var total = await ClinicalcasesQuery.CountAsync();

        var items = await ClinicalcasesQuery
            .OrderByDescending(x => x.CreatedAt) // BẮT BUỘC
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

}