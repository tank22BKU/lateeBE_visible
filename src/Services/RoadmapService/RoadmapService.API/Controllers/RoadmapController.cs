using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoadmapService.Application.Queries.GenerateRoadmap;
using RoadmapService.Application.Queries.GetClinicalCases;

namespace RoadmapService.API.Controllers;

[ApiController]
[Route("api/clinical-cases")]
public class ClinicalCasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicalCasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetClinicalCasesQuery(status, page, pageSize)
        );

        return Ok(result);
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