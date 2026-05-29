using MediatR;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Commands.DeleteExpert;

public sealed class DeleteExpertCommand : IRequest<bool>
{
    public string ExpertId { get; set; } = string.Empty;
}

public sealed class DeleteExpertHandler : IRequestHandler<DeleteExpertCommand, bool>
{
    private readonly IUserRepository _repository;

    public DeleteExpertHandler(IUserRepository repository) => _repository = repository;

    public Task<bool> Handle(DeleteExpertCommand request, CancellationToken cancellationToken)
    {
        return _repository.DeleteExpertAsync(request.ExpertId);
    }
}
