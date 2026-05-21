using ClinicalCaseService.Application.Commands.CreateClinicalCase;
using ClinicalCaseService.Application.Commands.DeleteClinicalCase;
using ClinicalCaseService.Application.Commands.UpdateClinicalCase;
using ClinicalCaseService.Application.Queries.GetClinicalCaseById;
using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicalCaseService.API.Controllers;

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
        [FromQuery] int pageSize = 20
    )
    {
        var result = await _mediator.Send(new GetClinicalCasesQuery(status, page, pageSize));

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
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.CaseId }, result);
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
