using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQuery : IRequest<User?>
{
    public string UserId { get; set; } = string.Empty;
}

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUserRepository _repository;

    public GetUserByIdHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetUserByIdAsync(request.UserId);
    }
}