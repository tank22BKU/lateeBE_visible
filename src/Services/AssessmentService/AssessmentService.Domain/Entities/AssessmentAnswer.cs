namespace AssessmentService.Domain.Entities;

public class AssessmentAnswer
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SessionId { get; set; } = null!;
    public string QuestionId { get; set; } = null!;
    public string? UserChoice { get; set; }
    public bool IsCorrect { get; set; }
    public decimal PointsEarned { get; set; }
    public bool IsFlagged { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}