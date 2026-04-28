namespace EvaluationService.Application.Dtos;

public class WarningDto
{
    public string WarningId { get; set; } = Guid.NewGuid().ToString("N");
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}