using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Users.Commands.CreateExpert;
using UserService.Application.Users.Commands.DeleteExpert;
using UserService.Application.Users.Commands.UpdateExpert;
using UserService.Application.Users.Queries.GetExpertById;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/experts")]
public class ExpertsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _repository;

    public ExpertsController(IMediator mediator, IUserRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword = null)
    {
        var experts = await _repository.GetExpertLookupsAsync(keyword);
        return Ok(experts);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var experts = await _repository.GetExpertLookupsAsync(keyword);
        return Ok(experts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var expert = await _mediator.Send(new GetExpertByIdQuery { ExpertId = id });
        return expert is null ? NotFound() : Ok(expert);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Expert req)
    {
        var created = await _mediator.Send(
            new CreateExpertCommand
            {
                ExpertId = req.ExpertId,
                Ssn = req.Ssn,
                BioQuote = req.BioQuote,
                EducationDetail = req.EducationDetail,
                TitlePosition = req.TitlePosition,
                ExpertiseSkill = req.ExpertiseSkill,
                SocialLink = req.SocialLink,
            }
        );

        return created is null
            ? BadRequest(new { message = "User not found for provided ExpertId" })
            : CreatedAtAction(nameof(Get), new { id = created.ExpertId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Expert req)
    {
        if (!string.Equals(id, req.ExpertId, StringComparison.OrdinalIgnoreCase))
            req.ExpertId = id;

        var updated = await _mediator.Send(
            new UpdateExpertCommand
            {
                ExpertId = req.ExpertId,
                Ssn = req.Ssn,
                BioQuote = req.BioQuote,
                EducationDetail = req.EducationDetail,
                TitlePosition = req.TitlePosition,
                ExpertiseSkill = req.ExpertiseSkill,
                SocialLink = req.SocialLink,
            }
        );

        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediator.Send(new DeleteExpertCommand { ExpertId = id });
        return deleted ? NoContent() : NotFound();
    }
}
