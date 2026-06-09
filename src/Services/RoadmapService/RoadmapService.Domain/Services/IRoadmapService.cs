namespace RoadmapService.Domain.Services;

/// <summary>
/// Service for interacting with Google Gemini API
/// </summary>
public interface IRoadmapService
{
    /// <summary>
    /// Sends a prompt to Gemini API and returns the response
    /// </summary>
    /// <param name="prompt">The prompt to send to Gemini</param>
    /// <returns>Response text from Gemini API</returns>
    Task<string> GenerateResponseAsync(string historyPractice, string userTarget, int amountOfTime);
}
