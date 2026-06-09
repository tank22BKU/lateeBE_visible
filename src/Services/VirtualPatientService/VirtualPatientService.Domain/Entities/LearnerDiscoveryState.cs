namespace VirtualPatientService.Domain.Entities;

public class LearnerDiscoveryState
{
    public string LearnerId { get; set; } = default!;
    public string? FilterJson { get; set; }
    public DateTime LastAccessed { get; set; }
}