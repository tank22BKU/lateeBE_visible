using System.Text.Json;
using System.Text.Json.Serialization;
using RoadmapService.Domain.Entities;

namespace RoadmapService.Application.Dtos.Response;

public class RoadmapResponse
{
    [JsonPropertyName("roadmap_id")]
    public string RoadmapId { get; set; } = string.Empty;

    [JsonPropertyName("learner_id")]
    public string LearnerId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    public static RoadmapResponse FromEntity(Roadmap roadmap)
    {
        return new RoadmapResponse
        {
            RoadmapId = roadmap.RoadmapId,
            LearnerId = roadmap.LearnerId,
            Content = ParseContent(roadmap.Content),
            Version = roadmap.Version,
            CreatedAt = roadmap.CreatedAt
        };
    }

    private static JsonElement ParseContent(string contentJson)
    {
        try
        {
            using var document = JsonDocument.Parse(contentJson);
            return document.RootElement.Clone();
        }
        catch
        {
            using var fallback = JsonDocument.Parse("{}");
            return fallback.RootElement.Clone();
        }
    }
}