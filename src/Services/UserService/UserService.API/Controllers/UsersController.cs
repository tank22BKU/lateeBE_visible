using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence;
using UserService.Domain.Entities;
using UserService.Application.DTOs;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;
    public UsersController(UserDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Users.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        return Ok(u);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserDto req)
    {
        var entity = new User
        {
            UserId = string.IsNullOrEmpty(req.UserId) ? Guid.NewGuid().ToString() : req.UserId,
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            Role = req.Role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.UserId }, entity);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UserDto req)
    {
        var existing = await _db.Users.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = req.Name;
        existing.Email = req.Email;
        existing.Phone = req.Phone;
        existing.Role = req.Role;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _db.Users.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Users.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
