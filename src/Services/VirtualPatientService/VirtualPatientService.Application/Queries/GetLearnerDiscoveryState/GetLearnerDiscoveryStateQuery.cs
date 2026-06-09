using MediatR;

namespace VirtualPatientService.Application.Queries.GetLearnerDiscoveryState;

public class GetLearnerDiscoveryStateQuery : IRequest<GetLearnerDiscoveryStateResponse>
{
    public string LearnerId { get; set; } = default!;
}