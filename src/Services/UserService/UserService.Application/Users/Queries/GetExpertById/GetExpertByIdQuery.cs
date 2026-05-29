using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Queries.GetExpertById;

public sealed class GetExpertByIdQuery : IRequest<Expert?>
{
    public string ExpertId { get; set; } = string.Empty;
}

public sealed class GetExpertByIdHandler : IRequestHandler<GetExpertByIdQuery, Expert?>
{
    private readonly IUserRepository _repository;

    public GetExpertByIdHandler(IUserRepository repository) => _repository = repository;

    public Task<Expert?> Handle(GetExpertByIdQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetExpertByIdAsync(request.ExpertId);
    }
}
