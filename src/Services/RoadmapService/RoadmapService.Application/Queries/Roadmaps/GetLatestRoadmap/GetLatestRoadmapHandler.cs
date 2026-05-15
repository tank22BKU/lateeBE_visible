using MediatR;
using RoadmapService.Application.Dtos.Response;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Application.Queries.Roadmaps.GetLatestRoadmap;

public class GetLatestRoadmapHandler : IRequestHandler<GetLatestRoadmapQuery, RoadmapResponse?>
{
    private readonly IRoadmapRepository _repository;

    public GetLatestRoadmapHandler(IRoadmapRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoadmapResponse?> Handle(GetLatestRoadmapQuery request, CancellationToken cancellationToken)
    {
        var roadmap = await _repository.GetLatestRoadmapAsync(request.LearnerId);
        return roadmap is null ? null : RoadmapResponse.FromEntity(roadmap);
    }
}