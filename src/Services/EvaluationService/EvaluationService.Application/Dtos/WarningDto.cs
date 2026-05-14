namespace EvaluationService.Application.Dtos;

public class WarningDto
{
    public string   WarningId         { get; set; } = Guid.NewGuid().ToString("N");
    public string   PracticeSessionId { get; set; } = string.Empty;
    public string   LearnerId         { get; set; } = string.Empty;
    public string   Label             { get; set; } = string.Empty;
    public string   Description       { get; set; } = string.Empty;
    public DateTime CreatedAt         { get; set; } = DateTime.UtcNow;
}