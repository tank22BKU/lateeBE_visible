using System.Text.Json;
using System.Text.Json.Serialization;
using RoadmapService.Domain.Services;
using RoadmapService.Application.Queries.GenerateRoadmap;
using Microsoft.Extensions.Logging;
using MediatR;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public class GenerateRoadmapHandler : IRequestHandler<GenerateRoadmapRequest, GenerateRoadmapResponse>
{
    private readonly IRoadmapService _roadmapService;
    private readonly ILogger<GenerateRoadmapHandler> _logger;

    public GenerateRoadmapHandler(IRoadmapService roadmapService, ILogger<GenerateRoadmapHandler> logger)
    {
        _roadmapService = roadmapService;
        _logger = logger;
    }

    public async Task<GenerateRoadmapResponse> Handle(GenerateRoadmapRequest q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q.HistoryPractice) ||
            string.IsNullOrWhiteSpace(q.UserTarget) ||
            q.TotalDaysAvailable <= 0)
        {
            throw new ArgumentException("Invalid input");
        }

        var raw = await _roadmapService.GenerateResponseAsync(
            q.HistoryPractice, q.UserTarget, q.TotalDaysAvailable);

        _logger.LogInformation("RAW LLM response: {raw}", raw);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            // =========================
            // 1. Parse outer response
            // =========================
            var hf = JsonSerializer.Deserialize<HuggingFaceResponse>(raw, options);

            // Try multiple fields because some HF models put text in different places
            var first = hf?.Choices?.FirstOrDefault();
            var content = first?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
                content = first?.Text;
            if (string.IsNullOrWhiteSpace(content))
                content = hf?.Text;
            // Only use reasoning_content as last resort if it contains actual JSON
            if (string.IsNullOrWhiteSpace(content) && first?.Message?.ReasoningContent?.Contains('{') == true)
                content = first.Message.ReasoningContent;

            _logger.LogInformation("Selected LLM content source length={Length}", content?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("All message content fields empty; falling back to raw response for JSON extraction");
                content = raw;
            }

            // =========================
            // 2. Extract and validate JSON
            // =========================
            var cleaned = ExtractValidRoadmapJson(content, _logger);

            _logger.LogInformation("CLEANED LLM response: {cleaned}", cleaned);

            // =========================
            // 3. Parse roadmap thật
            // =========================
            var dto = JsonSerializer.Deserialize<GenerateRoadmapResponse>(cleaned, options);

            if (dto?.Roadmap == null || dto.Roadmap.Count == 0)
                throw new Exception("Invalid roadmap");

            // =========================
            // 4. Normalize
            // =========================
            var normalized = dto.Roadmap
                .Where(x => !string.IsNullOrWhiteSpace(x.RecommendedContent))
                .Select((x, index) => new RoadmapItem
                {
                    OrderId = index + 1,
                    RecommendedContent = x.RecommendedContent.Trim(),
                    DetailedExplain = x.DetailedExplain?.Trim() ?? "",
                    AmountOfTimeDays = x.AmountOfTimeDays > 0 ? x.AmountOfTimeDays : 1
                })
                .ToList();

            if (normalized.Count == 0)
                throw new Exception("Invalid roadmap");

            if (q.TotalDaysAvailable < normalized.Count)
            {
                normalized = normalized
                    .Take(q.TotalDaysAvailable)
                    .Select((x, index) => new RoadmapItem
                    {
                        OrderId = index + 1,
                        RecommendedContent = x.RecommendedContent,
                        DetailedExplain = x.DetailedExplain,
                        AmountOfTimeDays = 1
                    })
                    .ToList();
            }

            var totalAssignedDays = normalized.Sum(x => x.AmountOfTimeDays);
            var dayDelta = q.TotalDaysAvailable - totalAssignedDays;

            if (dayDelta != 0)
            {
                // Adjust the last item so the sum always matches total_days requested by user.
                var last = normalized[^1];
                last.AmountOfTimeDays = Math.Max(1, last.AmountOfTimeDays + dayDelta);

                var overflow = normalized.Sum(x => x.AmountOfTimeDays) - q.TotalDaysAvailable;
                if (overflow > 0)
                {
                    for (var i = normalized.Count - 2; i >= 0 && overflow > 0; i--)
                    {
                        var reducible = normalized[i].AmountOfTimeDays - 1;
                        if (reducible <= 0)
                            continue;

                        var reduction = Math.Min(reducible, overflow);
                        normalized[i].AmountOfTimeDays -= reduction;
                        overflow -= reduction;
                    }
                }
            }

            return new GenerateRoadmapResponse
            {
                TotalDays = q.TotalDaysAvailable,
                RoadmapTitle = !string.IsNullOrEmpty(dto.RoadmapTitle) ? dto.RoadmapTitle.Trim() : "",
                Goal = !string.IsNullOrEmpty(dto.Goal) ? dto.Goal.Trim() : "",
                Roadmap = normalized
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR PARSE: " + ex.Message);

            return new GenerateRoadmapResponse
            {
                TotalDays = q.TotalDaysAvailable,
                RoadmapTitle = "Unable to generate roadmap",
                Goal = "Unable to generate roadmap",
                Roadmap = new List<RoadmapItem>
                {
                    new RoadmapItem
                    {
                        OrderId = 1,
                        RecommendedContent = "Unable to generate roadmap",
                        DetailedExplain = "LLM parsing failed",
                        AmountOfTimeDays = q.TotalDaysAvailable
                    }
                },
            };
        }
    }

    private string ExtractValidRoadmapJson(string input, ILogger<GenerateRoadmapHandler> logger)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // Find all potential JSON blocks (between { and })
        var jsonBlocks = ExtractJsonBlocks(input);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // Try each block to find one that deserializes to valid roadmap structure
        foreach (var block in jsonBlocks)
        {
            try
            {
                var test = JsonSerializer.Deserialize<GenerateRoadmapResponse>(block, options);
                if (test?.Roadmap != null && test.Roadmap.Count > 0)
                {
                    logger.LogInformation("Found valid roadmap JSON with {ItemCount} items", test.Roadmap.Count);
                    return block;
                }
            }
            catch (JsonException ex)
            {
                logger.LogDebug("JSON block not valid roadmap: {Error}", ex.Message);
                continue;
            }
        }

        logger.LogWarning("No valid roadmap JSON blocks found; returning raw input for last-chance parsing");
        return input;
    }

    private static List<string> ExtractJsonBlocks(string input)
    {
        var blocks = new List<string>();
        int start = input.IndexOf('{');

        while (start >= 0)
        {
            // Find matching closing brace using brace counting
            int braceCount = 1;
            int pos = start + 1;
            int end = -1;

            while (pos < input.Length && braceCount > 0)
            {
                if (input[pos] == '{')
                    braceCount++;
                else if (input[pos] == '}')
                    braceCount--;

                if (braceCount == 0)
                {
                    end = pos;
                    break;
                }
                pos++;
            }

            if (end > start)
            {
                blocks.Add(input.Substring(start, end - start + 1));
            }

            // Look for next opening brace
            start = input.IndexOf('{', start + 1);
        }

        return blocks;
    }
}

public class HuggingFaceResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class Message
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("reasoning_content")]
    public string ReasoningContent { get; set; } = "";
}