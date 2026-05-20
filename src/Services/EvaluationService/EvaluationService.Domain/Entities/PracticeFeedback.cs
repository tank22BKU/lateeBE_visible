namespace EvaluationService.Domain.Entities;

public class PracticeFeedback
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? OverallAttempt { get; set; }
    public string? OverallLabel { get; set; }
    public string? Strength { get; set; }
    public string? Improvement { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string EvaluationId { get; set; } = default!;
    public string PracticeSessionId { get; set; } = default!;
}
