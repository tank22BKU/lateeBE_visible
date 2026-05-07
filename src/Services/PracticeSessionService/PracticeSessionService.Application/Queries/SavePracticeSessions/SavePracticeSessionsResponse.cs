namespace PracticeSessionService.Application.Queries.SavePracticeSessions;
public class SavePracticeSessionsResponse
{
    public string SessionId { get; set; }

    public SavePracticeSessionsResponse(string sessionId)
    {
        SessionId = sessionId;
    }
}