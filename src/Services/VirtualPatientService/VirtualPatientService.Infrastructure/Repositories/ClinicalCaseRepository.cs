using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class ClinicalCaseRepository : IClinicalCaseRepository
{
    private readonly VirtualPatientDbContext _db;

    public ClinicalCaseRepository(VirtualPatientDbContext db) => _db = db;

    public Task<ClinicalCase?> GetByIdAsync(
        string caseId,
        CancellationToken cancellationToken = default
    ) =>
        _db
            .ClinicalCases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CaseId == caseId, cancellationToken);

    public async Task<Dictionary<string, ClinicalCase>> GetByIdsAsync(
        IEnumerable<string> caseIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = caseIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<string, ClinicalCase>();

        var items = await _db
            .ClinicalCases.AsNoTracking()
            .Where(x => ids.Contains(x.CaseId))
            .ToListAsync(cancellationToken);

        return items.ToDictionary(x => x.CaseId, x => x);
    }
}
