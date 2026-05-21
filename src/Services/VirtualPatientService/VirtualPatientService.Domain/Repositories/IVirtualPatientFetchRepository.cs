using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface IVirtualPatientFetchRepository
{
    Task<bool> LearnerExistsAsync(string learnerId, CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetOwnedPatientIdsAsync(
        string learnerId,
        CancellationToken cancellationToken = default
    );

    Task<List<VirtualPatientProjection>> GetAvailableCasesAsync(
        string? level,
        string? gender,
        IEnumerable<string> excludePatientIds,
        CancellationToken cancellationToken = default
    );

    Task<int> SaveFetchedCasesAsync(
        string learnerId,
        IEnumerable<string> patientIds,
        CancellationToken cancellationToken = default
    );
}
