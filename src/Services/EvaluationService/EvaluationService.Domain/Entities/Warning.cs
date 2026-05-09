namespace EvaluationService.Domain.Entities;

public class Warning
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PracticeSessionId { get; set; } = null!;
    public string LearnerId { get; set; } = null!;
    public string? Label { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
