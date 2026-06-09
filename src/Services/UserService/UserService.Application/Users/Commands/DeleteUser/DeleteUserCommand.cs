using MediatR;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
}

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUserRepository _repository;

    public DeleteUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return _repository.DeleteUserAsync(request.UserId);
    }
}