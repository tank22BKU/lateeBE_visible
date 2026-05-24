using System.Text;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class LearnerDiscoveryPoolRepository : ILearnerDiscoveryPoolRepository
{
    private readonly VirtualPatientDbContext _db;
    private readonly IPracticeAttemptRepository _attemptRepo;

    public LearnerDiscoveryPoolRepository(
        VirtualPatientDbContext db,
        IPracticeAttemptRepository attemptRepo
    )
    {
        _db = db;
        _attemptRepo = attemptRepo;
    }

    public async Task<IReadOnlyList<string>> GetExistingPatientIdsAsync(
        string learnerId,
        CancellationToken ct = default
    )
    {
        var ids = await _db
            .LearnerDiscoveryPools.AsNoTracking()
            .Where(x => x.LearnerId == learnerId)
            .Select(x => x.PatientId)
            .Distinct()
            .ToListAsync(ct);

        return ids;
    }

    public Task<int> GetPoolTotalAsync(string learnerId, CancellationToken ct = default) =>
        _db.LearnerDiscoveryPools.AsNoTracking().CountAsync(x => x.LearnerId == learnerId, ct);

    public async Task AddRangeAsync(
        IEnumerable<LearnerDiscoveryPool> entries,
        CancellationToken ct = default
    )
    {
        var items = entries.ToList();
        if (items.Count == 0)
            return;

        var sql = new StringBuilder();
        sql.Append(
            "INSERT IGNORE INTO learner_discovery_pool (id, learner_id, patient_id, fetched_at, fetch_level, fetch_gender) VALUES "
        );

        var parameters = new List<object>();
        for (var index = 0; index < items.Count; index++)
        {
            if (index > 0)
                sql.Append(", ");

            sql.Append(
                $"(@id{index}, @learnerId{index}, @patientId{index}, @fetchedAt{index}, @fetchLevel{index}, @fetchGender{index})"
            );

            parameters.Add(new MySqlParameter($"@id{index}", items[index].Id));
            parameters.Add(new MySqlParameter($"@learnerId{index}", items[index].LearnerId));
            parameters.Add(new MySqlParameter($"@patientId{index}", items[index].PatientId));
            parameters.Add(new MySqlParameter($"@fetchedAt{index}", items[index].FetchedAt));
            parameters.Add(
                new MySqlParameter(
                    $"@fetchLevel{index}",
                    (object?)items[index].FetchLevel ?? DBNull.Value
                )
            );
            parameters.Add(
                new MySqlParameter(
                    $"@fetchGender{index}",
                    (object?)items[index].FetchGender ?? DBNull.Value
                )
            );
        }

        await _db.Database.ExecuteSqlRawAsync(sql.ToString(), parameters.ToArray(), ct);
    }

    public async Task<IReadOnlyList<DiscoveryPatientProjection>> GetPoolItemsAsync(
        string learnerId,
        string sortBy,
        CancellationToken ct = default
    )
    {
        var baseRows = await (
            from pool in _db.LearnerDiscoveryPools.AsNoTracking()
            where pool.LearnerId == learnerId
            join vp in _db.VirtualPatients.AsNoTracking() on pool.PatientId equals vp.PatientId
            join cc in _db.ClinicalCases.AsNoTracking() on vp.CaseId equals cc.CaseId
            select new
            {
                vp.PatientId,
                vp.CaseId,
                vp.Name,
                vp.Age,
                vp.Gender,
                vp.Occupation,
                vp.ChiefConcern,
                cc.Symptom,
                vp.Level,
                vp.AvatarImage,
                vp.TimeSetting,
                vp.ArgumentTime,
                vp.CreatedAt,
            }
        ).ToListAsync(ct);

        if (baseRows.Count == 0)
            return Array.Empty<DiscoveryPatientProjection>();

        var patientIds = baseRows.Select(x => x.PatientId).Distinct().ToList();
        var attempts = await _attemptRepo.GetAttemptSummariesAsync(learnerId, patientIds, ct);
        var experts = await GetExpertsByPatientIdsAsync(patientIds, ct);

        var sorted = sortBy switch
        {
            VirtualPatientConstants.SortOptions.Oldest => baseRows.OrderBy(x => x.CreatedAt),
            VirtualPatientConstants.SortOptions.LevelAsc => baseRows.OrderBy(x => x.Level),
            VirtualPatientConstants.SortOptions.LevelDesc => baseRows.OrderByDescending(x =>
                x.Level
            ),
            _ => baseRows.OrderByDescending(x => x.CreatedAt),
        };

        return sorted
            .Select(x =>
            {
                attempts.TryGetValue(x.PatientId, out var attempt);
                experts.TryGetValue(x.PatientId, out var expertList);

                return new DiscoveryPatientProjection(
                    PatientId: x.PatientId,
                    CaseId: x.CaseId,
                    Name: x.Name,
                    Age: x.Age,
                    Gender: x.Gender,
                    Occupation: x.Occupation,
                    ChiefConcern: x.ChiefConcern,
                    Symptom: x.Symptom,
                    Level: x.Level,
                    AvatarImage: x.AvatarImage,
                    TimeSetting: x.TimeSetting,
                    ArgumentTime: x.ArgumentTime,
                    CreatedAt: x.CreatedAt,
                    AttemptSummary: new AttemptSummaryProjection(
                        Attempted: attempt?.Attempted ?? false,
                        AttemptCount: attempt?.AttemptCount ?? 0,
                        BestScore: attempt?.BestScore,
                        LatestScore: attempt?.LatestScore
                    ),
                    Experts: expertList ?? new List<DiscoveryExpertProjection>()
                );
            })
            .ToList();
    }

    public async Task<IReadOnlyList<DiscoveryPatientProjection>> GetRandomAvailableCasesAsync(
        string learnerId,
        string? level,
        string? gender,
        int fetchCount,
        CancellationToken ct = default
    )
    {
        var existingIds = await GetExistingPatientIdsAsync(learnerId, ct);
        var excludedIds = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query =
            from vp in _db.VirtualPatients.AsNoTracking()
            join cc in _db.ClinicalCases.AsNoTracking() on vp.CaseId equals cc.CaseId
            where vp.Status == VirtualPatientConstants.Status.Active
            select new
            {
                vp.PatientId,
                vp.CaseId,
                vp.Name,
                vp.Age,
                vp.Gender,
                vp.Occupation,
                vp.ChiefConcern,
                cc.Symptom,
                vp.Level,
                vp.AvatarImage,
                vp.TimeSetting,
                vp.ArgumentTime,
                vp.CreatedAt,
            };

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(x => x.Level == level);

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(x => x.Gender == gender);

        if (excludedIds.Count > 0)
            query = query.Where(x => !excludedIds.Contains(x.PatientId));

        var candidates = await query.ToListAsync(ct);
        var selected = candidates.OrderBy(_ => Random.Shared.Next()).Take(fetchCount).ToList();

        return selected
            .Select(x => new DiscoveryPatientProjection(
                PatientId: x.PatientId,
                CaseId: x.CaseId,
                Name: x.Name,
                Age: x.Age,
                Gender: x.Gender,
                Occupation: x.Occupation,
                ChiefConcern: x.ChiefConcern,
                Symptom: x.Symptom,
                Level: x.Level,
                AvatarImage: x.AvatarImage,
                TimeSetting: x.TimeSetting,
                ArgumentTime: x.ArgumentTime,
                CreatedAt: x.CreatedAt,
                AttemptSummary: new AttemptSummaryProjection(false, 0, null, null),
                Experts: new List<DiscoveryExpertProjection>()
            ))
            .ToList();
    }

    private async Task<
        Dictionary<string, List<DiscoveryExpertProjection>>
    > GetExpertsByPatientIdsAsync(
        IReadOnlyCollection<string> patientIds,
        CancellationToken ct = default
    )
    {
        if (patientIds.Count == 0)
            return new Dictionary<string, List<DiscoveryExpertProjection>>();

        var results = await (
            from evpm in _db.ExpertVirtualPatientManagements.AsNoTracking()
            where patientIds.Contains(evpm.VirtualId)
            join exp in _db.Experts.AsNoTracking() on evpm.ExpertId equals exp.ExpertId
            join u in _db.UserRefs.AsNoTracking() on exp.ExpertId equals u.UserId
            select new
            {
                evpm.VirtualId,
                Expert = new DiscoveryExpertProjection(
                    exp.ExpertId,
                    u.Name,
                    exp.TitlePosition,
                    u.AvatarUrl
                ),
            }
        ).ToListAsync(ct);

        return results
            .GroupBy(x => x.VirtualId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Expert).ToList());
    }
}
