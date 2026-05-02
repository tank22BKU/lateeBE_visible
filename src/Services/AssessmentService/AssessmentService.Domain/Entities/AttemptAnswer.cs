namespace AssessmentService.Domain.Entities;

public class AttemptAnswer
{
    public string AnswerId { get; set; } = Guid.NewGuid().ToString("N");
    public string AttemptId { get; set; } = null!;
    public string QuestionId { get; set; } = null!;
    public string UserChoice { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public decimal PointsEarned { get; set; }
}