namespace VirtualPatientService.Application.Dtos;

public class VirtualPatientDiscoveryItemDto
{
    public string PatientId { get; set; } = default!;
    public string CaseId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? ChiefConcern { get; set; }
    public string? Symptom { get; set; }
    public string? Level { get; set; }
    public string? AvatarImage { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FeedbackCount { get; set; }
    public AttemptSummaryDto AttemptSummary { get; set; } = new();
    public List<ExpertPreviewDto> Experts { get; set; } = new();
}
