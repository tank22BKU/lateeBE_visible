using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class VirtualPatientRepository : IVirtualPatientRepository
{
    private readonly VirtualPatientDbContext _db;

    public VirtualPatientRepository(VirtualPatientDbContext db) => _db = db;

    public Task<VirtualPatient?> GetByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default
    ) =>
        _db
            .VirtualPatients.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PatientId == patientId, cancellationToken);

    public async Task<(List<VirtualPatient> Items, int Total)> GetPagedAsync(
        string? gender,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = _db.VirtualPatients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(x => x.Gender == gender);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(List<VirtualPatientProjection> Items, int Total)> GetPagedForDiscoveryAsync(
        int page,
        int pageSize,
        string? level,
        string? occupation,
        string? expertId,
        string? gender,
        string? specialty,
        string? caseType,
        string? search,
        string sortBy,
        CancellationToken cancellationToken = default
    )
    {
        var query =
            from vp in _db.VirtualPatients.AsNoTracking()
            where vp.Status == "active"
            join cc in _db.ClinicalCases.AsNoTracking() on vp.CaseId equals cc.CaseId
            select new { vp, cc };

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(x => x.vp.Level == level);

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(x => x.vp.Gender == gender);

        if (!string.IsNullOrWhiteSpace(occupation))
            query = query.Where(x => EF.Functions.Like(x.vp.Occupation, $"%{occupation}%"));

        if (!string.IsNullOrWhiteSpace(caseType))
            query = query.Where(x => x.cc.Type == caseType);

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(x => EF.Functions.Like(x.cc.Description, $"%{specialty}%"));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(x =>
                EF.Functions.Like(x.vp.Name, pattern)
                || EF.Functions.Like(x.vp.ChiefConcern, pattern)
                || EF.Functions.Like(x.cc.Title, pattern)
            );
        }

        if (!string.IsNullOrWhiteSpace(expertId))
        {
            var linked = _db
                .ExpertVirtualPatientManagements.AsNoTracking()
                .Where(e => e.ExpertId == expertId)
                .Select(e => e.VirtualId);
            query = query.Where(x => linked.Contains(x.vp.PatientId));
        }

        var total = await query.CountAsync(cancellationToken);

        var sorted = sortBy switch
        {
            "oldest" => query.OrderBy(x => x.vp.CreatedAt),
            "level_asc" => query.OrderBy(x => x.vp.Level),
            "level_desc" => query.OrderByDescending(x => x.vp.Level),
            _ => query.OrderByDescending(x => x.vp.CreatedAt),
        };

        var items = await sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return (items, total);
    }

    public async Task<List<ExpertWithUserProjection>> GetExpertsByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default
    )
    {
        return await (
            from evpm in _db.ExpertVirtualPatientManagements.AsNoTracking()
            where evpm.VirtualId == patientId
            join exp in _db.Experts.AsNoTracking() on evpm.ExpertId equals exp.ExpertId
            join u in _db.UserRefs.AsNoTracking() on exp.ExpertId equals u.UserId
            select new ExpertWithUserProjection(
                exp.ExpertId,
                evpm.VirtualId,
                u.Name,
                exp.TitlePosition,
                u.AvatarUrl,
                exp.BioQuote,
                exp.EducationDetail,
                exp.ExpertiseSkill,
                u.Phone,
                u.Email
            )
        ).ToListAsync(cancellationToken);
    }

    public async Task<
        Dictionary<string, List<ExpertWithUserProjection>>
    > GetExpertsByPatientIdsAsync(
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = patientIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<string, List<ExpertWithUserProjection>>();

        var results = await (
            from evpm in _db.ExpertVirtualPatientManagements.AsNoTracking()
            where ids.Contains(evpm.VirtualId)
            join exp in _db.Experts.AsNoTracking() on evpm.ExpertId equals exp.ExpertId
            join u in _db.UserRefs.AsNoTracking() on exp.ExpertId equals u.UserId
            select new ExpertWithUserProjection(
                exp.ExpertId,
                evpm.VirtualId,
                u.Name,
                exp.TitlePosition,
                u.AvatarUrl,
                exp.BioQuote,
                exp.EducationDetail,
                exp.ExpertiseSkill,
                u.Phone,
                u.Email
            )
        ).ToListAsync(cancellationToken);

        return results.GroupBy(e => e.VirtualId).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<DiscoveryFiltersProjection> GetDiscoveryFiltersAsync(
        CancellationToken cancellationToken = default
    )
    {
        var levels = await _db
            .VirtualPatients.AsNoTracking()
            .Where(vp => vp.Status == "active" && vp.Level != null)
            .Select(vp => vp.Level!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(cancellationToken);

        var genders = await _db
            .VirtualPatients.AsNoTracking()
            .Where(vp => vp.Status == "active" && vp.Gender != null)
            .Select(vp => vp.Gender!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync(cancellationToken);

        var caseTypes = await _db
            .ClinicalCases.AsNoTracking()
            .Where(cc => cc.Type != null)
            .Select(cc => cc.Type!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(cancellationToken);

        return new DiscoveryFiltersProjection(
            AvailableLevels: levels,
            AvailableGenders: genders,
            AvailableSpecialties: new List<string>(),
            AvailableCaseTypes: caseTypes
        );
    }
}
