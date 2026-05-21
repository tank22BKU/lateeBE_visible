using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualPatientService.Application.Commands.FetchVirtualPatientCases;
using VirtualPatientService.Application.Commands.SaveLearnerDiscoveryState;
using VirtualPatientService.Application.Queries.GetLearnerDiscoveryState;
using VirtualPatientService.Application.Queries.GetVirtualPatientById;
using VirtualPatientService.Application.Queries.GetVirtualPatientDiscovery;
using VirtualPatientService.Application.Queries.GetVirtualPatients;

namespace VirtualPatientService.API.Controllers;

[ApiController]
[Route("api/virtual-patients")]
public class VirtualPatientController : ControllerBase
{
    private readonly IMediator _mediator;

    public VirtualPatientController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? gender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(
            new GetVirtualPatientsQuery(gender, page, pageSize),
            cancellationToken
        );

        return Ok(result);
    }

    [HttpGet("discovery")]
    public async Task<IActionResult> GetDiscovery(
        [FromQuery] string learnerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        [FromQuery] string? level = null,
        [FromQuery] string? occupation = null,
        [FromQuery] string? expertId = null,
        [FromQuery] string? gender = null,
        [FromQuery] string? specialty = null,
        [FromQuery] string? caseType = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _mediator.Send(
                new GetVirtualPatientDiscoveryQuery
                {
                    LearnerId = learnerId,
                    Page = page,
                    PageSize = pageSize,
                    Level = level,
                    Occupation = occupation,
                    ExpertId = expertId,
                    Gender = gender,
                    Specialty = specialty,
                    CaseType = caseType,
                    Search = search,
                    SortBy = sortBy,
                },
                cancellationToken
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("discovery/fetch-cases")]
    public async Task<IActionResult> FetchCases(
        [FromBody] FetchVirtualPatientCasesCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (LearnerNotFoundException ex)
        {
            return Unauthorized(
                new
                {
                    success = false,
                    errorCode = "LEARNER_NOT_FOUND",
                    message = ex.Message,
                }
            );
        }
        catch (NoMoreCasesAvailableException ex)
        {
            return NotFound(
                new
                {
                    success = false,
                    errorCode = "NO_MORE_CASES_AVAILABLE",
                    message = ex.Message,
                }
            );
        }
        catch (FetchCasesValidationException ex)
        {
            return BadRequest(
                new
                {
                    success = false,
                    errorCode = ex.ErrorCode,
                    message = ex.Message,
                }
            );
        }
    }

    [HttpGet("learner-last-discovery")]
    public async Task<IActionResult> GetLearnerDiscoveryState(
        [FromQuery] string learnerId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _mediator.Send(
                new GetLearnerDiscoveryStateQuery { LearnerId = learnerId },
                cancellationToken
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("learner-last-discovery")]
    public async Task<IActionResult> SaveLearnerDiscoveryState(
        [FromBody] SaveLearnerDiscoveryStateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _mediator.Send(new GetVirtualPatientByIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound(new { message = $"Không tìm thấy bệnh án với ID: {id}" });

        return Ok(result);
    }
}
