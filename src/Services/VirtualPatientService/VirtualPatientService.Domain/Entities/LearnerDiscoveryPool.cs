namespace VirtualPatientService.Domain.Entities;

public class LearnerDiscoveryPool
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LearnerId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public string? FetchLevel { get; set; }
    public string? FetchGender { get; set; }

    public VirtualPatient VirtualPatient { get; set; } = null!;
}
