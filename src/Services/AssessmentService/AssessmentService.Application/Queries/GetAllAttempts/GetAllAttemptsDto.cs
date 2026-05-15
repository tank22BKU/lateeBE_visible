namespace AssessmentService.Application.Queries.GetAllAttempts;

public class GetAllAttemptsDto
{
    public string AttemptId { get; set; } = string.Empty;
    public string AssessmentId { get; set; } = string.Empty;
    public string LearnerId { get; set; } = string.Empty;
    public int AttemptNo { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public int CorrectCount { get; set; }
    public int? Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
