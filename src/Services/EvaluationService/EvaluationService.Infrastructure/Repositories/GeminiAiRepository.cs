using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace EvaluationService.Infrastructure.Repositories;

public class GeminiAiRepository : IGeminiAiRepository
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public GeminiAiRepository(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["GeminiAi:ApiKey"] ?? config["GEMINI_API_KEY"];
    }

    public Task<List<EpaScore>> AnalyzePerformanceAsync(EvaluationResult res)
    {
        return AnalyzePerformanceInternalAsync(res);
    }

    private async Task<List<EpaScore>> AnalyzePerformanceInternalAsync(EvaluationResult res)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return BuildFallbackScores(res);
        }

        var prompt = BuildPrompt(res);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);

            var generatedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                return BuildFallbackScores(res);
            }

            var cleanJson = ExtractJson(generatedText);
            var parsed = JsonSerializer.Deserialize<GeminiEvaluationResponse>(
                cleanJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.EpaAssessments == null || parsed.EpaAssessments.Count == 0)
            {
                return BuildFallbackScores(res);
            }

            return parsed.EpaAssessments
                .Take(5)
                .Select(x => new EpaScore
                {
                    ScoreId = Guid.NewGuid().ToString("N"),
                    ResultId = res.ResultId,
                    EpaId = string.IsNullOrWhiteSpace(x.EpaId) ? "EPA_1" : x.EpaId,
                    EntrustmentLevel = Math.Clamp(x.EntrustmentLevel, 1, 5),
                    NumericalScore = Math.Clamp(x.Score, 0, 20),
                    FeedbackDetail = string.IsNullOrWhiteSpace(x.Feedback)
                        ? "No detailed feedback provided."
                        : x.Feedback.Trim()
                })
                .ToList();
        }
        catch
        {
            return BuildFallbackScores(res);
        }
    }

    private static string BuildPrompt(EvaluationResult res)
    {
        return $@"
You are an expert clinical evaluator.

Evaluate this learner's performance and return ONLY valid JSON with this exact schema:
{{
    ""epaAssessments"": [
        {{
        ""epaId"": ""EPA_1"",
        ""title"": ""Information Gathering"",
        ""score"": 0,
        ""entrustmentLevel"": 1,
        ""feedback"": ""...""
        }}
    ]
}}

Rules:
- Exactly 5 EPA objects: EPA_1..EPA_5 in order.
- Score per EPA is integer from 0 to 20.
- entrustmentLevel is integer from 1 to 5.
- feedback should be concise but specific and actionable.
- No markdown, no explanation outside JSON.

Learner data:
- SessionId: {res.SessionId}
- UserId: {res.UserId}
- ClinicalCaseId: {res.ClinicalCaseId}
- Diagnosis: {res.FinalDiagnosis}
- VP Conversation Log: {res.VpConversationLog}
- Reasoning Log: {res.AiReasoningLog}
";
    }

    private static string ExtractJson(string raw)
    {
        var text = raw.Trim();

        if (text.StartsWith("```") && text.Contains("\n"))
        {
            var firstNewline = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```");
            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                text = text.Substring(firstNewline + 1, lastFence - firstNewline - 1).Trim();
            }
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return text.Substring(start, end - start + 1);
        }

        return text;
    }

    private static List<EpaScore> BuildFallbackScores(EvaluationResult res)
    {
        var chatLog = res.VpConversationLog ?? string.Empty;
        var reasoningLog = res.AiReasoningLog ?? string.Empty;
        var diagnosis = res.FinalDiagnosis ?? string.Empty;

        return new List<EpaScore>
        {
            new()
            {
                ScoreId = Guid.NewGuid().ToString("N"),
                ResultId = res.ResultId,
                EpaId = "EPA_1",
                EntrustmentLevel = chatLog.Length > 400 ? 4 : 2,
                NumericalScore = chatLog.Length > 400 ? 12 : 8,
                FeedbackDetail = chatLog.Length > 400
                    ? "Learner gathered key symptom data and related findings with acceptable depth."
                    : "Information gathering is limited; expand history and associated symptom exploration."
            },
            new()
            {
                ScoreId = Guid.NewGuid().ToString("N"),
                ResultId = res.ResultId,
                EpaId = "EPA_2",
                EntrustmentLevel = reasoningLog.Length > 300 ? 4 : 2,
                NumericalScore = reasoningLog.Length > 300 ? 10 : 7,
                FeedbackDetail = reasoningLog.Length > 300
                    ? "Reasoning is coherent and differential diagnosis is mostly appropriate."
                    : "Differential reasoning is shallow; add justification and exclusion logic."
            },
            new()
            {
                ScoreId = Guid.NewGuid().ToString("N"),
                ResultId = res.ResultId,
                EpaId = "EPA_3",
                EntrustmentLevel = string.IsNullOrWhiteSpace(diagnosis) ? 1 : 3,
                NumericalScore = string.IsNullOrWhiteSpace(diagnosis) ? 4 : 10,
                FeedbackDetail = string.IsNullOrWhiteSpace(diagnosis)
                    ? "Diagnosis testing strategy is unclear because final diagnosis is missing."
                    : "Core tests are identified but can be strengthened with confirmatory plans."
            },
            new()
            {
                ScoreId = Guid.NewGuid().ToString("N"),
                ResultId = res.ResultId,
                EpaId = "EPA_4",
                EntrustmentLevel = 3,
                NumericalScore = 10,
                FeedbackDetail = "Management plan is reasonable but should include safety monitoring and escalation criteria."
            },
            new()
            {
                ScoreId = Guid.NewGuid().ToString("N"),
                ResultId = res.ResultId,
                EpaId = "EPA_5",
                EntrustmentLevel = 3,
                NumericalScore = 10,
                FeedbackDetail = "Patient education and shared decision-making are present but follow-up details are limited."
            }
        };
    }

    private sealed class GeminiEvaluationResponse
    {
        [JsonPropertyName("epaAssessments")]
        public List<GeminiEpaItem> EpaAssessments { get; set; } = [];
    }

    private sealed class GeminiEpaItem
    {
        [JsonPropertyName("epaId")]
        public string EpaId { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public decimal Score { get; set; }

        [JsonPropertyName("entrustmentLevel")]
        public int EntrustmentLevel { get; set; }

        [JsonPropertyName("feedback")]
        public string Feedback { get; set; } = string.Empty;
    }
}