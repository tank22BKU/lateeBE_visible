namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCase
{
    public string CaseId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public string? Pe { get; set; }
    public string? Symptom { get; set; }
    public string? MedicalHistory { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string EccId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
