using Microsoft.AspNetCore.Mvc;
using MediatR;
using VirtualPatientService.Application.Queries.GetVirtualPatientByID;
using VirtualPatientService.Application.Queries.GetVirtualPatients;

namespace VirtualPatientService.API.Controllers;

[ApiController]
[Route("api/virtual-patients")] 
public class VirtualPatientController : ControllerBase
{
    private readonly IMediator _mediator;

    public VirtualPatientController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet] 
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? gender,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(
            new GetVirtualPatientQuery(gender, page, pageSize)
        );

        return Ok(result);
    }

    [HttpGet("{id}")] 
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetVirtualPatientByIdQuery(id));
        
        if (result == null) 
            return NotFound(new { message = $"Không tìm thấy bệnh án với ID: {id}" });

        return Ok(result);
    }
}