namespace VirtualPatientService.Application.Queries.GetLearnerDiscoveryState;

public class GetLearnerDiscoveryStateResponse
{
    public string LearnerId { get; set; } = default!;
    public string? FilterJson { get; set; }
    public DateTime? LastAccessed { get; set; }
}