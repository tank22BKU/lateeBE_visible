namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class VirtualPatientDto
{
    public string PatientId { get; set; } = null!;
    public string CaseId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? ChiefConcern { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Symptom { get; set; }
    public string? Pronouns { get; set; }
    public string? Ethnicity { get; set; }
    public object? Persona { get; set; }
    public object? VitalSigns { get; set; }
    public object? Instructions { get; set; }
    public object? Behaviors { get; set; }
    public int? TimeSetting { get; set; }
    public int? ArgumentTime { get; set; }
    public object? LearningObjectives { get; set; }
    public string? Level { get; set; }
    public string? AvatarImage { get; set; }
    public object? CaseRule { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExpertDto> Experts { get; set; } = [];
}

public class ExpertDto
{
    public string ExpertId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
}
