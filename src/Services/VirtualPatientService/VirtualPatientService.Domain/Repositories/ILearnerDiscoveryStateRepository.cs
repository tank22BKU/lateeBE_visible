using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface ILearnerDiscoveryStateRepository
{
    Task<LearnerDiscoveryState?> GetByLearnerIdAsync(
        string learnerId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        LearnerDiscoveryState state,
        CancellationToken cancellationToken = default);
}