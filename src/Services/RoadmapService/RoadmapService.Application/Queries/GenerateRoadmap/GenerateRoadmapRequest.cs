using MediatR;

using RoadmapService.Application.Queries.GenerateRoadmap;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public record GenerateRoadmapRequest(
    string Prompt
) : IRequest<GenerateRoadmapResponse>;