namespace ClinicalCaseService.Application.Queries.GetClinicalCases;

using ClinicalCaseService.Domain.Entities;

public class ClinicalCaseDto
{
    public string CaseId { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? CaseType { get; set; }
    public string? Status { get; set; }
    public string? Pe { get; set; }
    public string? Symptom { get; set; }
    public string? MedicalHistory { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string? CreatedByName { get; set; }
    public string EccId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int VirtualPatientCount { get; set; }
    public int AttemptCount { get; set; }
    public decimal AvgScore { get; set; }
    public List<ClinicalCaseLab> Labs { get; set; } = [];
    public List<ClinicalCaseRadiology> Radiology { get; set; } = [];
    public List<ClinicalCaseVirtualPatient> VirtualPatients { get; set; } = [];
    public ClinicalCaseStats Stats { get; set; } = new();
}
