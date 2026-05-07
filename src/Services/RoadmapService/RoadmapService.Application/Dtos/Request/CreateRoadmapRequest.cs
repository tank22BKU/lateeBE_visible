using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using RoadmapService.Application.Dtos.Response;

namespace RoadmapService.Application.Dtos.Request;

public class CreateRoadmapRequest : IRequest<RoadmapResponse>
{
    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }

    [JsonPropertyName("learner_id")]
    public string LearnerId { get; set; } = string.Empty;
}