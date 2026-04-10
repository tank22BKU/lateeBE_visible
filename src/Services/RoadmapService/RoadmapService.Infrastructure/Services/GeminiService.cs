using Google.GenAI;
using System.Threading.Tasks;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoadmapService.Domain.Services;

namespace RoadmapService.Infrastructure.Services;

/// <summary>
/// Implementation of Gemini API service
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly Client _client;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _model;

    public GeminiService(
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("Gemini API key is missing");

        _client = new Client(apiKey: apiKey);
        _logger = logger;
        _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    }

    /// <summary>
    /// Sends a prompt to Gemini API and retrieves the response
    /// </summary>
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        try
        {
            _logger.LogInformation("Sending prompt to Gemini API with model: {Model}", _model);

            string content = Prompts.BuildGenerateRoadmapPrompt("","");

            var response = await _client.Models.GenerateContentAsync(
                model: _model,
                contents: prompt);

            var responseText = response?.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            if (!string.IsNullOrWhiteSpace(responseText))
            {
                _logger.LogInformation("Successfully received response from Gemini API");
                return responseText;
            }

            _logger.LogWarning("Gemini API returned empty response");
            return "No response generated";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw;
        }
    }
}
