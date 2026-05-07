using Microsoft.AspNetCore.Mvc;
using MediatR;
using PracticeSessionService.Application.Queries.GetPracticeSessions;
using PracticeSessionService.Application.Queries.SavePracticeSessions;
using PracticeSessionService.Application.Commands.CreatePracticeSession;
using PracticeSessionService.Application.Queries.GetClinicalCases;

namespace PracticeSessionService.API.Controllers;

[ApiController]
[Route("api/practice-sessions")]
public class PracticeSessionController : ControllerBase
{
    private readonly IMediator _mediator;

    public PracticeSessionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePracticeSession([FromBody] CreatePracticeSessionCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
    [HttpPost("submit")]
    public async Task<IActionResult> SavePracticeSession(
        [FromBody] SavePracticeSessionsRequest request
    ){
        var result = await _mediator.Send(
            request
        );

        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPracticeSessionWithId(string id)
    {
        var result = await _mediator.Send(
            new GetPracticeSessionsRequest
            {
                SessionId = id
            }
        );

        return Ok(result);
    }

    [HttpGet("clinical-cases")]
    public async Task<IActionResult> GetClinicalCases(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetClinicalCasesRequest
            {
                Status = status,
                Page = page,
                PageSize = pageSize
            }
        );

        return Ok(result);
    }
}