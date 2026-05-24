using ClinicalCaseService.Application.Commands.CreateClinicalCase;
using ClinicalCaseService.Application.Commands.DeleteClinicalCase;
using ClinicalCaseService.Application.Commands.UpdateClinicalCase;
using ClinicalCaseService.Application.Queries.GetClinicalCaseById;
using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;
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
        if (string.IsNullOrWhiteSpace(command.CreatedBy))
        {
            return BadRequest(new { message = "createdBy is required in the request body." });
        }

        try
        {
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = result.CaseId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateClinicalCaseCommand command)
    {
        if (!string.Equals(id, command.CaseId, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "ID trên route và body phải khớp nhau." });
        }

        var updated = await _mediator.Send(command);

        if (!updated)
        {
            return NotFound(new { message = $"Không tìm thấy clinical case với ID: {id}" });
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateClinicalCaseStatusRequest request)
    {
        var current = await _mediator.Send(new GetClinicalCaseByIdQuery(id));

        if (current == null)
        {
            return NotFound(new { message = $"Không tìm thấy clinical case với ID: {id}" });
        }

        var updated = await _mediator.Send(
            new UpdateClinicalCaseCommand(
                id,
                current.Title ?? string.Empty,
                current.Description,
                current.CaseType,
                request.Status,
                current.Pe,
                current.Symptom,
                current.MedicalHistory,
                current.CreatedBy,
                current.EccId
            )
        );

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

}

public sealed class UpdateClinicalCaseStatusRequest
{
    public string Status { get; set; } = null!;
}
