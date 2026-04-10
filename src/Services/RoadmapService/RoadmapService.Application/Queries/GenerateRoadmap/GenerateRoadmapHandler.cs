using RoadmapService.Domain.Services;
using RoadmapService.Application.Queries.GenerateRoadmap;
using MediatR;

namespace RoadmapService.Application.Queries.GenerateRoadmap;

public class GenerateRoadmapHandler : IRequestHandler<GenerateRoadmapRequest, GenerateRoadmapResponse>
{
    private readonly IGeminiService _geminiService;

    public GenerateRoadmapHandler(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<GenerateRoadmapResponse> Handle(GenerateRoadmapRequest q, CancellationToken cancellationToken)
    {
        if (q.Prompt == null)
        {
            throw new ArgumentNullException("Prompt cannot be null or empty", nameof(q.Prompt));
        }
        
        var response = await _geminiService.GenerateResponseAsync(q.Prompt);
        return new GenerateRoadmapResponse
        {
            Result = response
        };
    }
}