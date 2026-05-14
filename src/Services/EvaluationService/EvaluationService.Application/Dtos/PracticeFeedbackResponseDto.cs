namespace EvaluationService.Application.Dtos;

public class PracticeFeedbackResponseDto
{
    public string   Id             { get; set; } = string.Empty;
    public string?  OverallAttempt { get; set; }
    public string?  OverallLabel   { get; set; }
    public string?  Strength       { get; set; }
    public string?  Improvement    { get; set; }
    public DateTime CreatedAt      { get; set; }
    public bool     WasCached      { get; set; }
}