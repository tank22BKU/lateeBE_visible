using KnowledgeResourceService.Domain.Repositories;
using MediatR;

namespace KnowledgeResourceService.Application.KnowledgeResources.Commands.DeleteKnowledgeResource;

public sealed class DeleteKnowledgeResourceCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}

public sealed class DeleteKnowledgeResourceHandler : IRequestHandler<DeleteKnowledgeResourceCommand, bool>
{
    private readonly IKnowledgeResourceRepository _repository;

    public DeleteKnowledgeResourceHandler(IKnowledgeResourceRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(DeleteKnowledgeResourceCommand request, CancellationToken cancellationToken)
    {
        return _repository.DeleteKnowledgeResourceAsync(request.Id);
    }
}