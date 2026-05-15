using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Domain.Repositories;
using MediatR;

namespace KnowledgeResourceService.Application.KnowledgeResources.Queries.GetKnowledgeResourceById;

public sealed class GetKnowledgeResourceByIdQuery : IRequest<KnowledgeResource?>
{
    public string Id { get; set; } = string.Empty;
}

public sealed class GetKnowledgeResourceByIdHandler : IRequestHandler<GetKnowledgeResourceByIdQuery, KnowledgeResource?>
{
    private readonly IKnowledgeResourceRepository _repository;

    public GetKnowledgeResourceByIdHandler(IKnowledgeResourceRepository repository)
    {
        _repository = repository;
    }

    public Task<KnowledgeResource?> Handle(GetKnowledgeResourceByIdQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetKnowledgeResourceByIdAsync(request.Id);
    }
}