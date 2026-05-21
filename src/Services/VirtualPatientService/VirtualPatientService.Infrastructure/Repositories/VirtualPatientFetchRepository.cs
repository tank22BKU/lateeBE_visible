using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class VirtualPatientFetchRepository : IVirtualPatientFetchRepository
{
    private readonly VirtualPatientDbContext _db;

    public VirtualPatientFetchRepository(VirtualPatientDbContext db) => _db = db;

    public Task<bool> LearnerExistsAsync(
        string learnerId,
        CancellationToken cancellationToken = default
    ) => _db.UserRefs.AsNoTracking().AnyAsync(x => x.UserId == learnerId, cancellationToken);

    public async Task<HashSet<string>> GetOwnedPatientIdsAsync(
        string learnerId,
        CancellationToken cancellationToken = default
    )
    {
        var patientIds = await _db
            .PracticeSessionRefs.AsNoTracking()
            .Where(x => x.LearnerId == learnerId)
            .Select(x => x.PatientId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return patientIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<VirtualPatientProjection>> GetAvailableCasesAsync(
        string? level,
        string? gender,
        IEnumerable<string> excludePatientIds,
        CancellationToken cancellationToken = default
    )
    {
        var excludedIds = excludePatientIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var query =
            from vp in _db.VirtualPatients.AsNoTracking()
            where vp.Status == VirtualPatientConstants.Status.Active
            join cc in _db.ClinicalCases.AsNoTracking() on vp.CaseId equals cc.CaseId
            select new { vp, cc };

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(x => x.vp.Level == level);

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(x => x.vp.Gender == gender);

        if (excludedIds.Count > 0)
            query = query.Where(x => !excludedIds.Contains(x.vp.PatientId));

        return await query
            .OrderByDescending(x => x.vp.CreatedAt)
            .Select(x => new VirtualPatientProjection(
                x.vp.PatientId,
                x.vp.CaseId,
                x.vp.Name,
                x.vp.Age,
                x.vp.Gender,
                x.vp.Occupation,
                x.vp.ChiefConcern,
                x.cc.Symptom,
                x.vp.Level,
                x.vp.AvatarImage,
                x.vp.TimeSetting,
                x.vp.ArgumentTime,
                x.vp.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> SaveFetchedCasesAsync(
        string learnerId,
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTime.UtcNow;
        var items = patientIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(patientId => new PracticeSessionRef
            {
                Id = Guid.NewGuid().ToString("N"),
                LearnerId = learnerId,
                PatientId = patientId,
                Status = VirtualPatientConstants.PracticeStatus.Abandoned,
                CreatedAt = now,
            })
            .ToList();

        if (items.Count == 0)
            return 0;

        _db.PracticeSessionRefs.AddRange(items);
        await _db.SaveChangesAsync(cancellationToken);
        return items.Count;
    }
}
