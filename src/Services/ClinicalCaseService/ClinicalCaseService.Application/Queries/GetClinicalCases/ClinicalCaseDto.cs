namespace ClinicalCaseService.Application.Queries.GetClinicalCases;

public class ClinicalCaseDto
{
    public string Id { get; set; } = null!;
    public string? Title { get; set; }
    public string? Type { get; set; }
}
