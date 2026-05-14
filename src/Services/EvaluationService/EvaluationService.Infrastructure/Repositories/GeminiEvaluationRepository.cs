using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EvaluationService.Infrastructure.Repositories;

public sealed class GeminiEvaluationRepository : IAiEvaluationProvider
{
    private readonly HttpClient  _httpClient;
    private readonly string?     _apiKey;
    private readonly ILogger<GeminiEvaluationRepository> _logger;

    private const string ModelEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public GeminiEvaluationRepository(
        HttpClient                           httpClient,
        IConfiguration                       config,
        ILogger<GeminiEvaluationRepository>  logger)
    {
        _httpClient = httpClient;
        _apiKey     = config["GeminiAi:ApiKey"] ?? config["GEMINI_API_KEY"];
        _logger     = logger;
    }

    public async Task<GeminiEvaluationOutput> AnalyzePerformanceAsync(
        string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key missing — using fallback output.");
            return BuildFallbackOutput();
        }

        var url  = $"{ModelEndpoint}?key={_apiKey}";
        var body = new
        {
            contents         = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.1, responseMimeType = "application/json" }
        };

        try
        {
            var content  = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(raw);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Gemini returned empty text — using fallback.");
                return BuildFallbackOutput();
            }

            return ParseResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed.");
            return BuildFallbackOutput();
        }
    }

    private static GeminiEvaluationOutput ParseResponse(string raw)
    {
        var clean = ExtractJson(raw);
        var opts  = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parsed = JsonSerializer.Deserialize<GeminiSchema>(clean, opts);

        if (parsed?.EpaAssessments == null || parsed.EpaAssessments.Count == 0)
            return BuildFallbackOutput();

        var epaScores = parsed.EpaAssessments
            .Take(5)
            .Select((x, i) => new EvaluationEpaScore   
            {
                Id               = Guid.NewGuid().ToString("N"),
                EpaId            = string.IsNullOrWhiteSpace(x.EpaId) ? $"EPA_{i + 1}" : x.EpaId,
                NumericalScore   = Math.Clamp(x.Score, 0, 20),
                EntrustmentLevel = Math.Clamp(x.EntrustmentLevel, 1, 5),
                FeedbackDetail   = x.Feedback?.Trim() ?? "No feedback.",
                EvidenceCited    = x.EvidenceCited   ?? [],
                FailurePatterns  = x.FailurePatterns  ?? [],
                SafetyFlags      = x.SafetyFlags      ?? []
            })
            .ToList();

        return new GeminiEvaluationOutput(
            EpaScores:               epaScores,
            DiagnosisModifier:       parsed.DiagnosisModifier,
            DiagnosisMatchType:      parsed.DiagnosisMatchType ?? "UNKNOWN",
            TimeModifier:            parsed.TimeModifier,
            TotalWarningPenalty:     parsed.TotalWarningPenalty,
            FinalScore:              Math.Clamp(parsed.FinalScore, 0, 110),
            OverallEntrustmentLevel: Math.Clamp(parsed.OverallEntrustmentLevel, 1, 5),
            CognitiveAlerts:         parsed.CognitiveAlerts ?? [],
            SafetyEscalationRequired: parsed.SafetyEscalationRequired,
            EvaluationTrace:         parsed.EvaluationTrace ?? string.Empty
        );
    }

    private static GeminiEvaluationOutput BuildFallbackOutput() => new(
        EpaScores: Enumerable.Range(1, 5).Select(i => new EvaluationEpaScore
        {
            Id               = Guid.NewGuid().ToString("N"),
            EpaId            = $"EPA{i}",
            NumericalScore   = 10,
            EntrustmentLevel = 2,
            FeedbackDetail   = "AI evaluation unavailable — fallback score applied.",
            EvidenceCited    = [],
            FailurePatterns  = [],
            SafetyFlags      = []
        }).ToList(),
        DiagnosisModifier:       0,
        DiagnosisMatchType:      "UNVERIFIED",
        TimeModifier:            0,
        TotalWarningPenalty:     0,
        FinalScore:              50,
        OverallEntrustmentLevel: 2,
        CognitiveAlerts:         [],
        SafetyEscalationRequired: false,
        EvaluationTrace:         "Fallback evaluation — Gemini API unavailable."
    );

    private static string ExtractJson(string raw)
    {
        var text = raw.Trim();
        if (text.StartsWith("```"))
        {
            var first = text.IndexOf('\n');
            var last  = text.LastIndexOf("```");
            if (first >= 0 && last > first)
                text = text[(first + 1)..last].Trim();
        }
        var start = text.IndexOf('{');
        var end   = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    private sealed class GeminiSchema
    {
        [JsonPropertyName("epaAssessments")]       public List<GeminiEpaItem>? EpaAssessments  { get; set; }
        [JsonPropertyName("diagnosisModifier")]    public int    DiagnosisModifier              { get; set; }
        [JsonPropertyName("diagnosisMatchType")]   public string? DiagnosisMatchType            { get; set; }
        [JsonPropertyName("timeModifier")]         public int    TimeModifier                   { get; set; }
        [JsonPropertyName("totalWarningPenalty")]  public int    TotalWarningPenalty            { get; set; }
        [JsonPropertyName("cognitiveAlerts")]      public List<string>? CognitiveAlerts         { get; set; }
        [JsonPropertyName("finalScore")]           public int    FinalScore                     { get; set; }
        [JsonPropertyName("overallEntrustmentLevel")] public int OverallEntrustmentLevel        { get; set; }
        [JsonPropertyName("safetyEscalationRequired")] public bool SafetyEscalationRequired    { get; set; }
        [JsonPropertyName("evaluationTrace")]      public string? EvaluationTrace               { get; set; }
    }

    private sealed class GeminiEpaItem
    {
        [JsonPropertyName("epaId")]           public string  EpaId            { get; set; } = string.Empty;
        [JsonPropertyName("score")]           public int     Score            { get; set; }
        [JsonPropertyName("entrustmentLevel")] public int   EntrustmentLevel  { get; set; }
        [JsonPropertyName("feedback")]        public string? Feedback          { get; set; }
        [JsonPropertyName("evidenceCited")]   public List<string>? EvidenceCited  { get; set; }
        [JsonPropertyName("failurePatterns")] public List<string>? FailurePatterns { get; set; }
        [JsonPropertyName("safetyFlags")]     public List<string>? SafetyFlags     { get; set; }
    }
}