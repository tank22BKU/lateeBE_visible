using System.Text.Json.Serialization;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public class GenerateRoadmapResponse
{
    [JsonPropertyName("total_days")]
    public int TotalDays { get; set; }
    
    [JsonPropertyName("roadmap_title")]
    public string RoadmapTitle { get; set; } = String.Empty;
    
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = String.Empty;

    public List<RoadmapItem> Roadmap { get; set; } = new();
}

public class RoadmapItem
{
    [JsonPropertyName("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("recommended_content")]
    public string RecommendedContent { get; set; } = String.Empty;

    [JsonPropertyName("detailed_explain")]
    public string DetailedExplain { get; set; } = String.Empty;
    
    [JsonPropertyName("amount_of_time_days")]
    public int AmountOfTimeDays { get; set; }
}