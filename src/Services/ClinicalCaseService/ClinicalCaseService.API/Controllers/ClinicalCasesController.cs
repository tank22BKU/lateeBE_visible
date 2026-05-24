using System.IdentityModel.Tokens.Jwt;
using ClinicalCaseService.Application.Commands.CreateClinicalCase;
using ClinicalCaseService.Application.Commands.DeleteClinicalCase;
using ClinicalCaseService.Application.Commands.UpdateClinicalCase;
using ClinicalCaseService.Application.Queries.GetClinicalCaseById;
using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalCaseService.API.Controllers;

[ApiController]
[Route("api/expert/clinical-cases")]
public class ClinicalCasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicalCasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] string? eccid,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12
    )
    {
        var result = await _mediator.Send(
            new GetClinicalCasesQuery(search, status, type, eccid, sortBy, sortDir, page, pageSize)
        );

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetClinicalCaseByIdQuery(id));

        if (result == null)
        {
            return NotFound(new { message = $"Không tìm thấy clinical case với ID: {id}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClinicalCaseCommand command)
    {
        var createdBy = ResolveExpertId(command.CreatedBy);

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return BadRequest(
                new
                {
                    message = "CreatedBy is required when no authenticated expert identity is available.",
                }
            );
        }

        var result = await _mediator.Send(command with { CreatedBy = createdBy });

        return CreatedAtAction(nameof(GetById), new { id = result.CaseId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateClinicalCaseCommand command)
    {
        if (!string.Equals(id, command.CaseId, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "ID trên route và body phải khớp nhau." });
        }

        var createdBy = ResolveExpertId(command.CreatedBy);
        var updated = await _mediator.Send(command with { CreatedBy = createdBy });

        if (!updated)
        {
            return NotFound(new { message = $"Không tìm thấy clinical case với ID: {id}" });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediator.Send(new DeleteClinicalCaseCommand(id));

        if (!deleted)
        {
            return NotFound(new { message = $"Không tìm thấy clinical case với ID: {id}" });
        }

        return NoContent();
    }

    private string? ResolveExpertId(string? fallback)
    {
        var claimValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue;
        }

        return string.IsNullOrWhiteSpace(fallback) ? null : fallback;
    }
}
