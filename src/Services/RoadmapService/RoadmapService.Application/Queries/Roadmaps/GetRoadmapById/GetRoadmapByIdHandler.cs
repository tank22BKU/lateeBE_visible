using MediatR;
using RoadmapService.Application.Dtos.Response;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Application.Queries.Roadmaps.GetRoadmapById;

public class GetRoadmapByIdHandler : IRequestHandler<GetRoadmapByIdQuery, RoadmapResponse?>
{
    private readonly IRoadmapRepository _repository;

    public GetRoadmapByIdHandler(IRoadmapRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoadmapResponse?> Handle(GetRoadmapByIdQuery request, CancellationToken cancellationToken)
    {
        var roadmap = await _repository.GetRoadmapByIdAsync(request.RoadmapId);
        return roadmap is null ? null : RoadmapResponse.FromEntity(roadmap);
    }
}