namespace PracticeSessionService.Domain.Entities;

public class ClinicalCase
{
	public string CaseId { get; set; } = default!;

	public string Title { get; set; } = default!;

	public string? Description { get; set; }

	public string? Type { get; set; }

	public string? Status { get; set; }

	public string? Pe { get; set; }

	public string? Symptom { get; set; }

	public string? MedicalHistory { get; set; }

	public string? CreatedBy { get; set; }

	public string? EccId { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime UpdatedAt { get; set; }
}