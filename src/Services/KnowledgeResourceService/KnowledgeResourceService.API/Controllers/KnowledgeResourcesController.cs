using MediatR;
using KnowledgeResourceService.Application.KnowledgeResources.Commands.CreateKnowledgeResource;
using KnowledgeResourceService.Application.KnowledgeResources.Commands.DeleteKnowledgeResource;
using KnowledgeResourceService.Application.KnowledgeResources.Commands.UpdateKnowledgeResource;
using KnowledgeResourceService.Application.KnowledgeResources.Queries.GetAllKnowledgeResources;
using KnowledgeResourceService.Application.KnowledgeResources.Queries.GetKnowledgeResourceById;
using KnowledgeResourceService.Infrastructure.Persistence;
using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeResourceService.API.Controllers;

[ApiController]
[Route("api/knowledge-resources")]
public class KnowledgeResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public KnowledgeResourcesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _mediator.Send(new GetAllKnowledgeResourcesQuery());
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _mediator.Send(new GetKnowledgeResourceByIdQuery { Id = id });
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeResourceDto req)
    {
        var entity = await _mediator.Send(new CreateKnowledgeResourceCommand
        {
            Id = req.Id,
            Title = req.Title,
            Content = req.Content,
            AuthorId = req.AuthorId
        });

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, KnowledgeResourceDto req)
    {
        var updated = await _mediator.Send(new UpdateKnowledgeResourceCommand
        {
            Id = id,
            Title = req.Title,
            Content = req.Content,
            AuthorId = req.AuthorId
        });

        return updated is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediator.Send(new DeleteKnowledgeResourceCommand { Id = id });
        return deleted ? NoContent() : NotFound();
    }
}
