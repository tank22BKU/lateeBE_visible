namespace VirtualPatientService.Domain.Entities;

public class ClinicalCase
{
	public string CaseId { get; set; } = null!;
	public string? Description { get; set; }
	public string? Symptom { get; set; }
	public string? MedicalHistory { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}