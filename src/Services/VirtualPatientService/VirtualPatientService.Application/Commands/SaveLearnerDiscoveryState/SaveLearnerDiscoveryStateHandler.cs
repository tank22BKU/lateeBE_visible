using MediatR;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Commands.SaveLearnerDiscoveryState;

public class SaveLearnerDiscoveryStateHandler
    : IRequestHandler<SaveLearnerDiscoveryStateCommand, SaveLearnerDiscoveryStateResponse>
{
    private readonly ILearnerDiscoveryStateRepository _repo;

    public SaveLearnerDiscoveryStateHandler(ILearnerDiscoveryStateRepository repo) => _repo = repo;

    public async Task<SaveLearnerDiscoveryStateResponse> Handle(
        SaveLearnerDiscoveryStateCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required");

        var lastAccessed = request.LastAccessed ?? DateTime.UtcNow;

        var state = new LearnerDiscoveryState
        {
            LearnerId = request.LearnerId,
            FilterJson = request.FilterJson,
            LastAccessed = lastAccessed,
        };

        await _repo.UpsertAsync(state, cancellationToken);

        return new SaveLearnerDiscoveryStateResponse
        {
            Success = true,
            LearnerId = request.LearnerId,
            LastAccessed = lastAccessed,
        };
    }
}
