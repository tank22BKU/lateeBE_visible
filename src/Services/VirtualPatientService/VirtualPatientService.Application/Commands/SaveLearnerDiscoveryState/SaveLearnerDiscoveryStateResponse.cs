namespace VirtualPatientService.Application.Commands.SaveLearnerDiscoveryState;

public class SaveLearnerDiscoveryStateResponse
{
    public bool Success { get; set; }
    public string LearnerId { get; set; } = default!;
    public DateTime LastAccessed { get; set; }
}