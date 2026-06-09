namespace PracticeSessionService.Application.Queries.GetClinicalCases;

public class ClinicalCaseDto
{
    public string Id { get; set; } = default!;
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
}
