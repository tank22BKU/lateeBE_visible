namespace AssessmentService.Domain.Entities;

public class AssessmentSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string AssessmentId { get; set; } = null!;
    public decimal OverallScore { get; set; }
    public string LearnerId { get; set; } = null!;
    public int AttemptNo { get; set; } = 1;
    public int? Duration { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "InProgress";
    public bool? IsPassed { get; set; }

    public ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
}
