namespace AssessmentService.Domain.Entities;

public class AssessmentAttempt
{
    public string AttemptId { get; set; } = Guid.NewGuid().ToString("N");
    public string AssessmentId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public string Status { get; set; } = "InProgress";

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}
