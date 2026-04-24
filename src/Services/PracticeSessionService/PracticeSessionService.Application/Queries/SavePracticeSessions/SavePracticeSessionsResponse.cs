namespace PracticeSessionService.Application.Queries.SavePracticeSessions;
public class SavePracticeSessionsResponse
{
    public string ResultId { get; set; }

    public SavePracticeSessionsResponse(string resultId)
    {
        this.ResultId = resultId;
    }
}