namespace RoadmapService.Infrastructure.Services;

public class Prompts
{
    public static string GenerateRoadmapPrompt = "";

    public static string BuildGenerateRoadmapPrompt(string practiceSession, string feedbackFromSystemEvaluation)
    {
        if (string.IsNullOrWhiteSpace(practiceSession) || string.IsNullOrWhiteSpace(feedbackFromSystemEvaluation))
        {
            return "";
        }
        
        return "";
    }
}
