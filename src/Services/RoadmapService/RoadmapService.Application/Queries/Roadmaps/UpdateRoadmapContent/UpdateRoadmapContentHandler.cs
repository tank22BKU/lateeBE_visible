using System.Text.Json;
using MediatR;
using RoadmapService.Application.Dtos.Request;
using RoadmapService.Application.Dtos.Response;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Application.Queries.Roadmaps.UpdateRoadmapContent;

public class UpdateRoadmapContentHandler : IRequestHandler<UpdateRoadmapContentRequest, RoadmapResponse?>
{
    private readonly IRoadmapRepository _repository;

    public UpdateRoadmapContentHandler(IRoadmapRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoadmapResponse?> Handle(UpdateRoadmapContentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoadmapId))
        {
            throw new ArgumentException("roadmap_id is required");
        }

        if (request.Content.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException("content is required");
        }

        var roadmap = await _repository.UpdateRoadmapContentAsync(request.RoadmapId, request.Content.GetRawText());
        return roadmap is null ? null : RoadmapResponse.FromEntity(roadmap);
    }
}