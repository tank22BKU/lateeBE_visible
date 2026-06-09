using MediatR;
using VirtualPatientService.Domain.Repositories;

namespace VirtualPatientService.Application.Queries.GetLearnerDiscoveryState;

public class GetLearnerDiscoveryStateHandler
    : IRequestHandler<GetLearnerDiscoveryStateQuery, GetLearnerDiscoveryStateResponse>
{
    private readonly ILearnerDiscoveryStateRepository _repo;

    public GetLearnerDiscoveryStateHandler(ILearnerDiscoveryStateRepository repo) => _repo = repo;

    public async Task<GetLearnerDiscoveryStateResponse> Handle(
        GetLearnerDiscoveryStateQuery request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required");

        var state = await _repo.GetByLearnerIdAsync(request.LearnerId, cancellationToken);

        return new GetLearnerDiscoveryStateResponse
        {
            LearnerId = request.LearnerId,
            FilterJson = state?.FilterJson,
            LastAccessed = state?.LastAccessed,
        };
    }
}
