using ClinicalCaseService.Domain.Entities;
using ClinicalCaseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ClinicalCaseService.Domain.Repositories;

namespace ClinicalCaseService.Infrastructure.Repositories;

public class ClinicalCaseRepository : IClinicalCaseRepository
{
    private readonly ClinicalCaseDbContext _db;

    public ClinicalCaseRepository(ClinicalCaseDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClinicalCase>> GetAllAsync()
    {
        return await _db.ClinicalCases
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<ClinicalCase?> GetByIdAsync(string caseId)
    {
        return _db.ClinicalCases
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CaseId == caseId);
    }

    public async Task AddAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Add(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Update(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Remove(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<ClinicalCase> Items, int Total)>
    GetPagedAsync(string? status, int page, int pageSize)
    {
        var clinicalCasesQuery = _db.ClinicalCases.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
        {
            clinicalCasesQuery = clinicalCasesQuery.Where(x => x.Status == status);
        }

        var total = await clinicalCasesQuery.CountAsync();

        var items = await clinicalCasesQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}