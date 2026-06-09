using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommand : IRequest<User>
{
    public string? UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; } = "Male";
    public string? Address { get; set; } = "Not set yet";
    public string? AvatarUrl { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
}

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            UserId = request.UserId ?? string.Empty,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Birthday = request.Birthday,
            Gender = request.Gender,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Password = request.Password is null ? "user" : request.Password,
            AvatarUrl = request.AvatarUrl,
            Role = request.Role,
            Status = request.Status
        };

        return _repository.CreateUserAsync(entity);
    }
}