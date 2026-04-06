namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCase
{
    public string ClinicalCaseId { get; set; } = null!;   // VARCHAR(20) PK
    public string PatientId { get; set; } = null!;        // FK

    public string? Title { get; set; }
    public string? CaseType { get; set; }
    public string? Descriptions { get; set; }
    public string? Symptom { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Pe { get; set; }

    public string Status { get; set; } = "active";
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
