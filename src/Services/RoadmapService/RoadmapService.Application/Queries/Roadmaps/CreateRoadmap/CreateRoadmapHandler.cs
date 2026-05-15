using System.Text.Json;
using MediatR;
using RoadmapService.Application.Dtos.Request;
using RoadmapService.Application.Dtos.Response;
using RoadmapService.Domain.Entities;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Application.Queries.Roadmaps.CreateRoadmap;

public class CreateRoadmapHandler : IRequestHandler<CreateRoadmapRequest, RoadmapResponse>
{
    private readonly IRoadmapRepository _repository;

    public CreateRoadmapHandler(IRoadmapRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoadmapResponse> Handle(CreateRoadmapRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
        {
            throw new ArgumentException("learner_id is required");
        }

        if (request.Content.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException("content is required");
        }

        var roadmap = new Roadmap
        {
            RoadmapId = Guid.NewGuid().ToString("N"),
            LearnerId = request.LearnerId.Trim(),
            Content = request.Content.GetRawText(),
            Version = "1",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateRoadmapAsync(roadmap);
        return RoadmapResponse.FromEntity(created);
    }
}