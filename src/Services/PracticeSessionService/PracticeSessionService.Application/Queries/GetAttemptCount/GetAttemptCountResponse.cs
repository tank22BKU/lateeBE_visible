namespace PracticeSessionService.Application.Queries.GetAttemptCount;

public class GetAttemptCountResponse
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public bool CanAttempt { get; set; }
}
