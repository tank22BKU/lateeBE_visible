using System.Text.Json.Serialization;
using MediatR;
using RoadmapService.Application.Dtos.Response;

namespace RoadmapService.Application.Queries.Roadmaps.GetLatestRoadmap;

public class GetLatestRoadmapQuery : IRequest<RoadmapResponse?>
{
        [JsonIgnore]
        public string LearnerId { get; set; } = string.Empty;
}