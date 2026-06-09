namespace AssessmentService.Application.Queries.GetAllAttempts;

public class AssessmentAttemptOverview
{
    public string AttemptId { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public int CorrectCount { get; set; }
    public int Duration { get; set; }
    public decimal PassingScorePercentage { get; set; }
}   