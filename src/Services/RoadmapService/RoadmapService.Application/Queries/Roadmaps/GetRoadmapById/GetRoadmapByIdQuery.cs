using MediatR;
using RoadmapService.Application.Dtos.Response;

namespace RoadmapService.Application.Queries.Roadmaps.GetRoadmapById;

public record GetRoadmapByIdQuery(string RoadmapId) : IRequest<RoadmapResponse?>;