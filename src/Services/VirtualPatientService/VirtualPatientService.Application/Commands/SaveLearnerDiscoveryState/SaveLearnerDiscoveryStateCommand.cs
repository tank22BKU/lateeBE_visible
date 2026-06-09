using MediatR;

namespace VirtualPatientService.Application.Commands.SaveLearnerDiscoveryState;

public class SaveLearnerDiscoveryStateCommand : IRequest<SaveLearnerDiscoveryStateResponse>
{
    public string LearnerId { get; set; } = default!;
    public string? FilterJson { get; set; }
    public DateTime? LastAccessed { get; set; }
}