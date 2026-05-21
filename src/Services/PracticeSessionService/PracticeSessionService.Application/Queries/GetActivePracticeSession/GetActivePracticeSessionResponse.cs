namespace PracticeSessionService.Application.Queries.GetActivePracticeSession;

public class GetActivePracticeSessionResponse
{
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public string PatientId { get; set; } = default!;
}
