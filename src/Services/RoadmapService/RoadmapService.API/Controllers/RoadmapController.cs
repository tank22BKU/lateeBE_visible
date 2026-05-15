using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoadmapService.Application.Dtos.Request;
using RoadmapService.Application.Queries.GenerateRoadmap;
using RoadmapService.Application.Queries.Roadmaps.CreateRoadmap;
using RoadmapService.Application.Queries.Roadmaps.GetLatestRoadmap;
using RoadmapService.Application.Queries.Roadmaps.GetRoadmapById;
using RoadmapService.Application.Queries.Roadmaps.UpdateRoadmapContent;

namespace RoadmapService.API.Controllers;

[ApiController]
[Route("api/roadmap")]
public class RoadmapController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadmapController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoadmap([FromBody] CreateRoadmapRequest request)
    {
        var result = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetRoadmapById), new { roadmapId = result.RoadmapId }, result);
    }

    [HttpGet("{roadmapId}")]
    public async Task<IActionResult> GetRoadmapById([FromRoute] string roadmapId)
    {
        var result = await _mediator.Send(new GetRoadmapByIdQuery(roadmapId));

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{roadmapId}/content")]
    public async Task<IActionResult> UpdateRoadmapContent(
        [FromRoute] string roadmapId,
        [FromBody] UpdateRoadmapContentRequest request)
    {
        request.RoadmapId = roadmapId;

        var result = await _mediator.Send(request);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("latest/{learnerId}")]
    public async Task<IActionResult> GetLatestRoadmap([FromRoute] string learnerId)
    {
        var result = await _mediator.Send(new GetLatestRoadmapQuery
        {
            LearnerId = learnerId
        });

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("generate-roadmap")]
    public async Task<IActionResult> GenerateRoadmap([FromBody] GenerateRoadmapRequest request)
    {
        var result = await _mediator.Send(
            request
        );

        return Ok(result);
    }
}