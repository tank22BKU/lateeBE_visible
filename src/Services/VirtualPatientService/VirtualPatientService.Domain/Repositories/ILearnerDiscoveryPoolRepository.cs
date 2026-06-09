using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface ILearnerDiscoveryPoolRepository
{
    Task<IReadOnlyList<string>> GetExistingPatientIdsAsync(
        string learnerId,
        CancellationToken ct = default
    );

    Task<int> GetPoolTotalAsync(string learnerId, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<LearnerDiscoveryPool> entries, CancellationToken ct = default);

    Task<IReadOnlyList<DiscoveryPatientProjection>> GetPoolItemsAsync(
        string learnerId,
        string sortBy,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<DiscoveryPatientProjection>> GetRandomAvailableCasesAsync(
        string learnerId,
        string? level,
        string? gender,
        int fetchCount,
        CancellationToken ct = default
    );
}

public record DiscoveryPatientProjection(
    string PatientId,
    string CaseId,
    string Name,
    int? Age,
    string? Gender,
    string? Occupation,
    string? ChiefConcern,
    string? Symptom,
    string? Level,
    string? AvatarImage,
    int? TimeSetting,
    int? ArgumentTime,
    DateTime CreatedAt,
    AttemptSummaryProjection AttemptSummary,
    List<DiscoveryExpertProjection> Experts
);

public record DiscoveryExpertProjection(
    string ExpertId,
    string? Name,
    string? Role,
    string? AvatarUrl
);
