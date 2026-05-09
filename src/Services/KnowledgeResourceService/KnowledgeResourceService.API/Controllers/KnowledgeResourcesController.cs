using KnowledgeResourceService.Infrastructure.Persistence;
using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeResourceService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeResourcesController : ControllerBase
{
    private readonly KnowledgeDbContext _db;

    public KnowledgeResourcesController(KnowledgeDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.KnowledgeResources.ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _db.KnowledgeResources.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(KnowledgeResourceDto req)
    {
        var entity = new KnowledgeResource
        {
            Id = string.IsNullOrEmpty(req.Id) ? Guid.NewGuid().ToString() : req.Id,
            Title = req.Title,
            Content = req.Content,
            AuthorId = req.AuthorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.KnowledgeResources.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, KnowledgeResourceDto req)
    {
        var existing = await _db.KnowledgeResources.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Title = req.Title;
        existing.Content = req.Content;
        existing.AuthorId = req.AuthorId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _db.KnowledgeResources.FindAsync(id);
        if (existing == null) return NotFound();
        _db.KnowledgeResources.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
