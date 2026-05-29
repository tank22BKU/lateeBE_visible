using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Users.Commands.CreateUser;
using UserService.Application.Users.Commands.UpdateAvatarImage;
using UserService.Application.Users.Commands.DeleteUser;
using UserService.Application.Users.Commands.UpdateUser;
using UserService.Application.Users.Queries.GetAllUsers;
using UserService.Application.Users.Queries.GetDashboardStatistics;
using UserService.Application.Users.Queries.GetUserById;

namespace UserService.API.Controllers;

public sealed class UpdateAvatarRequest
{
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery { UserId = id });
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var stats = await _mediator.Send(new GetDashboardStatisticsQuery());
        return Ok(stats);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserDto req)
    {
        var created = await _mediator.Send(
            new CreateUserCommand
            {
                UserId = req.UserId,
                Name = req.Name,
                Email = req.Email,
                Password = req.Password,
                Phone = req.Phone,
                Birthday = req.Birthday,
                Gender = req.Gender,
                Address = req.Address,
                AvatarUrl = req.AvatarUrl,
                Status = req.Status,
                Role = req.Role,
            }
        );

        return CreatedAtAction(nameof(Get), new { id = created.UserId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UserDto req)
    {
        var updated = await _mediator.Send(
            new UpdateUserCommand
            {
                UserId = id,
                Name = req.Name,
                Email = req.Email,
                Phone = req.Phone,
                Role = req.Role,
                Password = req.Password,
                Birthday = req.Birthday,
                Gender = req.Gender,
                Address = req.Address,
                AvatarUrl = req.AvatarUrl,
                Status = req.Status,
            }
        );

        return updated is null ? NotFound() : NoContent();
    }

    [HttpPost("{id}/avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateAvatar(string id, [FromForm] UpdateAvatarRequest request)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        await using var stream = request.File.OpenReadStream();

        var updated = await _mediator.Send(new UpdateAvatarImageCommand
        {
            UserId = id,
            FileStream = stream,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType
        });

        return updated is null ? NotFound() : Ok(new { imageUrl = updated.AvatarUrl });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediator.Send(new DeleteUserCommand { UserId = id });
        return deleted ? NoContent() : NotFound();
    }
}
