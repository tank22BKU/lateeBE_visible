using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using RoadmapService.Application.Dtos.Response;

namespace RoadmapService.Application.Dtos.Request;

public class UpdateRoadmapContentRequest : IRequest<RoadmapResponse?>
{
    [JsonIgnore]
    public string RoadmapId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }
}