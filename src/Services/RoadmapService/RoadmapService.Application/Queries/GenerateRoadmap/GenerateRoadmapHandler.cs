using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadmapService.Domain.Services;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public sealed class GenerateRoadmapHandler
    : IRequestHandler<GenerateRoadmapRequest, GenerateRoadmapResponse>
{
    private readonly IRoadmapService _roadmapService;
    private readonly ILogger<GenerateRoadmapHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public GenerateRoadmapHandler(
        IRoadmapService roadmapService,
        ILogger<GenerateRoadmapHandler> logger)
    {
        _roadmapService = roadmapService;
        _logger = logger;
    }

    public async Task<GenerateRoadmapResponse> Handle(
        GenerateRoadmapRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        try
        {
            var rawResponse = await _roadmapService.GenerateResponseAsync(
                request.HistoryPractice,
                request.UserTarget,
                request.TotalDaysAvailable);

            _logger.LogInformation(
                "Received LLM response. Length={Length}",
                rawResponse.Length);

            var llmContent = ExtractLlmContent(rawResponse);

            if (string.IsNullOrWhiteSpace(llmContent))
            {
                throw new InvalidOperationException(
                    "LLM content is empty.");
            }

            var extractedJson = ExtractFirstJsonObject(llmContent);

            if (string.IsNullOrWhiteSpace(extractedJson))
            {
                throw new InvalidOperationException(
                    "No JSON object found.");
            }

            var repairedJson = RepairJson(extractedJson);

            var parsed = ParseRoadmapSafely(repairedJson);

            var normalized = NormalizeRoadmap(
                parsed,
                request.TotalDaysAvailable);

            return normalized;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Roadmap generation failed.");

            return BuildFallbackResponse(
                request.TotalDaysAvailable);
        }
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private static void ValidateRequest(
        GenerateRoadmapRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HistoryPractice))
        {
            throw new ArgumentException(
                "HistoryPractice is required.");
        }

        if (string.IsNullOrWhiteSpace(request.UserTarget))
        {
            throw new ArgumentException(
                "UserTarget is required.");
        }

        if (request.TotalDaysAvailable <= 0)
        {
            throw new ArgumentException(
                "TotalDaysAvailable must be > 0.");
        }
    }

    // =========================================================
    // EXTRACT CONTENT
    // =========================================================

    private string ExtractLlmContent(string raw)
    {
        try
        {
            var response = JsonSerializer.Deserialize<
                HuggingFaceResponse>(raw, JsonOptions);

            if (response is null)
            {
                return raw;
            }

            var firstChoice = response.Choices.FirstOrDefault();

            if (firstChoice?.FinishReason == "length")
            {
                _logger.LogWarning(
                    "LLM output truncated due to token limit.");
            }

            var content =
                firstChoice?.Message?.Content
                ?? firstChoice?.Text
                ?? response.Text
                ?? raw;

            return content.Trim();
        }
        catch
        {
            return raw;
        }
    }

    // =========================================================
    // JSON EXTRACTION
    // =========================================================

    private static string ExtractFirstJsonObject(
        string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var start = input.IndexOf('{');

        if (start < 0)
        {
            return string.Empty;
        }

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < input.Length; i++)
        {
            var ch = input[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
            {
                depth++;
            }
            else if (ch == '}')
            {
                depth--;

                if (depth == 0)
                {
                    return input[start..(i + 1)];
                }
            }
        }

        return input[start..];
    }

    // =========================================================
    // JSON REPAIR
    // =========================================================

    private static string RepairJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        var sb = new StringBuilder();

        var inString = false;
        var escape = false;

        foreach (var ch in json)
        {
            sb.Append(ch);

            if (escape)
            {
                escape = false;
                continue;
            }

            if (ch == '\\')
            {
                escape = true;
                continue;
            }

            if (ch == '"')
            {
                inString = !inString;
            }
        }

        if (inString)
        {
            sb.Append('"');
        }

        var repaired = sb.ToString();

        var openBraces = repaired.Count(c => c == '{');
        var closeBraces = repaired.Count(c => c == '}');

        if (openBraces > closeBraces)
        {
            repaired += new string(
                '}',
                openBraces - closeBraces);
        }

        var openBrackets = repaired.Count(c => c == '[');
        var closeBrackets = repaired.Count(c => c == ']');

        if (openBrackets > closeBrackets)
        {
            repaired += new string(
                ']',
                openBrackets - closeBrackets);
        }

        return repaired;
    }

    // =========================================================
    // SAFE PARSE
    // =========================================================

    private GenerateRoadmapResponse ParseRoadmapSafely(
        string json)
    {
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        var roadmap = new List<RoadmapItem>();

        if (root.TryGetProperty("roadmap", out var roadmapElement))
        {
            foreach (var item in roadmapElement.EnumerateArray())
            {
                try
                {
                    var roadmapItem =
                        JsonSerializer.Deserialize<RoadmapItem>(
                            item.GetRawText(),
                            JsonOptions);

                    if (roadmapItem is null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(
                            roadmapItem.RecommendedContent))
                    {
                        continue;
                    }

                    roadmap.Add(roadmapItem);
                }
                catch
                {
                    // Skip broken item only
                }
            }
        }

        return new GenerateRoadmapResponse
        {
            RoadmapTitle =
                GetString(root, "roadmap_title"),

            Goal =
                GetString(root, "goal"),

            TotalDays =
                GetInt(root, "total_days"),

            Roadmap = roadmap
        };
    }

    // =========================================================
    // NORMALIZATION
    // =========================================================

    private GenerateRoadmapResponse NormalizeRoadmap(
        GenerateRoadmapResponse response,
        int requestedDays)
    {
        var normalized = response.Roadmap
            .Where(x =>
                !string.IsNullOrWhiteSpace(
                    x.RecommendedContent))
            .Select((x, index) => new RoadmapItem
            {
                OrderId = index + 1,

                RecommendedContent =
                    x.RecommendedContent.Trim(),

                DetailedExplain =
                    x.DetailedExplain?.Trim() ?? "",

                AmountOfTimeDays =
                    x.AmountOfTimeDays > 0
                        ? x.AmountOfTimeDays
                        : 1
            })
            .ToList();

        if (normalized.Count == 0)
        {
            throw new InvalidOperationException(
                "No valid roadmap items.");
        }

        var currentDays =
            normalized.Sum(x => x.AmountOfTimeDays);

        var delta = requestedDays - currentDays;

        normalized[^1].AmountOfTimeDays =
            Math.Max(
                1,
                normalized[^1].AmountOfTimeDays + delta);

        return new GenerateRoadmapResponse
        {
            TotalDays = requestedDays,

            Goal = response.Goal,

            RoadmapTitle = response.RoadmapTitle,

            Roadmap = normalized
        };
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static string GetString(
        JsonElement root,
        string property)
    {
        if (root.TryGetProperty(property, out var value))
        {
            return value.GetString() ?? "";
        }

        return "";
    }

    private static int GetInt(
        JsonElement root,
        string property)
    {
        if (!root.TryGetProperty(property, out var value))
        {
            return 0;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetInt32(),

            JsonValueKind.String =>
                int.TryParse(
                    value.GetString(),
                    out var parsed)
                    ? parsed
                    : 0,

            _ => 0
        };
    }

    private static GenerateRoadmapResponse BuildFallbackResponse(
        int totalDays)
    {
        return new GenerateRoadmapResponse
        {
            TotalDays = totalDays,

            RoadmapTitle =
                "Unable to generate roadmap",

            Goal =
                "Unable to generate roadmap",

            Roadmap =
            [
                new RoadmapItem
                {
                    OrderId = 1,

                    RecommendedContent =
                        "Unable to generate roadmap",

                    DetailedExplain =
                        "LLM parsing failed",

                    AmountOfTimeDays =
                        totalDays
                }
            ]
        };
    }
}

// =========================================================
// DTOs
// =========================================================

public sealed class HuggingFaceResponse
{
    [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = [];

    [JsonPropertyName("text")] public string? Text { get; set; }
}

public sealed class Choice
{
    [JsonPropertyName("message")] public Message? Message { get; set; }

    [JsonPropertyName("text")] public string? Text { get; set; }

    [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
}

public sealed class Message
{
    [JsonPropertyName("content")] public string? Content { get; set; }

    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }
}