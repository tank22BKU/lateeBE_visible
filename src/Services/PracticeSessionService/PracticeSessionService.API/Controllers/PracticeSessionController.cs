using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracticeSessionService.Application.Commands.CreatePracticeSession;
using PracticeSessionService.Application.Commands.UpdatePracticeSessionStatus;
using PracticeSessionService.Application.Queries.GetActivePracticeSession;
using PracticeSessionService.Application.Queries.GetAttemptCount;
using PracticeSessionService.Application.Queries.GetClinicalCases;
using PracticeSessionService.Application.Queries.GetPracticeSessions;
using PracticeSessionService.Application.Queries.SavePracticeSessions;

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
    [ProducesResponseType(typeof(CreatePracticeSessionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePracticeSession(
        [FromBody] CreatePracticeSessionCommand request
    )
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPost("submit")]
    [ProducesResponseType(typeof(SavePracticeSessionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SavePracticeSession(
        [FromBody] SavePracticeSessionsRequest request
    )
    {
        var result = await _mediator.Send(request);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetPracticeSessionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPracticeSessionWithId(string id)
    {
        var result = await _mediator.Send(new GetPracticeSessionsRequest { SessionId = id });

        return Ok(result);
    }

    [HttpGet("clinical-cases")]
    [ProducesResponseType(typeof(PagedResult<ClinicalCaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClinicalCases(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var result = await _mediator.Send(
            new GetClinicalCasesRequest
            {
                Status = status,
                Page = page,
                PageSize = pageSize,
            }
        );

        return Ok(result);
    }

    [HttpGet("attempt-count")]
    [ProducesResponseType(typeof(GetAttemptCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAttemptCount(
        [FromQuery] string learnerId,
        [FromQuery] string patientId
    )
    {
        try
        {
            var result = await _mediator.Send(
                new GetAttemptCountRequest { LearnerId = learnerId, PatientId = patientId }
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(GetActivePracticeSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveSession(
        [FromQuery] string learnerId,
        [FromQuery] string patientId
    )
    {
        try
        {
            var result = await _mediator.Send(
                new GetActivePracticeSessionRequest { LearnerId = learnerId, PatientId = patientId }
            );

            if (result == null)
                return NotFound(new { message = "Active practice session not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("by-patient")]
    [ProducesResponseType(
        typeof(PracticeSessionService.Application.Queries.GetPracticeSessionsByPatient.GetPracticeSessionsByPatientResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPracticeSessionsByPatient(
        [FromQuery] string learnerId,
        [FromQuery] string patientId
    )
    {
        try
        {
            var result = await _mediator.Send(
                new PracticeSessionService.Application.Queries.GetPracticeSessionsByPatient.GetPracticeSessionsByPatientRequest
                {
                    LearnerId = learnerId,
                    PatientId = patientId,
                }
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(UpdatePracticeSessionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdatePracticeSessionStatusRequest request
    )
    {
        try
        {
            var result = await _mediator.Send(
                new UpdatePracticeSessionStatusCommand { SessionId = id, Status = request.Status }
            );

            if (result == null)
                return NotFound(new { message = "Practice session not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
