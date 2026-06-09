using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommand : IRequest<User?>
{
    public string UserId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; } = "Male";
    public string? Address { get; set; } = "Not set yet";
    public string? AvatarUrl { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
}

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, User?>
{
    private readonly IUserRepository _repository;

    public UpdateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            UserId = request.UserId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Birthday = request.Birthday,
            Gender = request.Gender,
            Address = request.Address,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            Password = request.Password,
            AvatarUrl = request.AvatarUrl,
            Role = request.Role,
            Status = request.Status
        };

        return _repository.UpdateUserAsync(entity);
    }
}