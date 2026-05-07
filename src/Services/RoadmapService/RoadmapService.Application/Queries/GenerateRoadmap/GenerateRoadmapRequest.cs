using MediatR;

using RoadmapService.Application.Queries.GenerateRoadmap;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public class GenerateRoadmapRequest : IRequest<GenerateRoadmapResponse>
{
    public required string HistoryPractice { get; init; }
    public required string UserTarget { get; init; }
    public required int TotalDaysAvailable { get; init; }
};