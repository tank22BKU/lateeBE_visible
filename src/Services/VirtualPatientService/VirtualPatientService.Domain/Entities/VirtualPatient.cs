namespace VirtualPatientService.Domain.Entities;

public class VirtualPatient
{
    public string PatientId { get; set; } = null!;
    public string CaseId { get; set; } = null!;
    public string? OwnerExpertId { get; set; }
    public string Name { get; set; } = null!;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Pronouns { get; set; }
    public string? Ethnicity { get; set; }
    public string? Persona { get; set; }
    public string? ChiefConcern { get; set; }
    public string? VitalSigns { get; set; }
    public string? Instructions { get; set; }
    public string? Behaviors { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public string? LearningObjectives { get; set; }
    public string? Level { get; set; }
    public string? AvatarImage { get; set; }
    public string? CaseRule { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
