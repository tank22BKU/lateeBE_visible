using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadmapService.Application.Dtos.Request;
using RoadmapService.Application.Dtos.Response;
using RoadmapService.Domain.Entities;
using RoadmapService.Domain.Repositories;

namespace RoadmapService.Application.Queries.Roadmaps.CreateRoadmap;

public class CreateRoadmapHandler : IRequestHandler<CreateRoadmapRequest, RoadmapResponse>
{
    private readonly IRoadmapRepository _repository;
    private readonly ILogger<CreateRoadmapHandler> _logger;

    public CreateRoadmapHandler(IRoadmapRepository repository, ILogger<CreateRoadmapHandler> logger)
    {
        _repository = repository;
        _logger = logger;
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

        try
        {
            var evaluationIds = await GetUnsummarizedEvaluationIdsAsync(
                request.LearnerId,
                cancellationToken);

            if (evaluationIds.Count > 0)
            {
                await _repository.AddSummarizeRoadmapsAsync(
                    created.RoadmapId,
                    evaluationIds,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write summarize_roadmap entries for roadmap {RoadmapId}.",
                created.RoadmapId);
        }

        return RoadmapResponse.FromEntity(created);
    }

    private async Task<List<string>> GetUnsummarizedEvaluationIdsAsync(
        string learnerId,
        CancellationToken cancellationToken)
    {
        var rows = await _repository.GetUnsummarizedEvaluationHistoryAsync(
            learnerId,
            cancellationToken);

        return rows
            .Select(x => x.EvaluationId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}