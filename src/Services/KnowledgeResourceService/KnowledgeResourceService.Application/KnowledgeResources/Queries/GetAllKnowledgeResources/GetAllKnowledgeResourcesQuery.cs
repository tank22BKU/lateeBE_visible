using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Domain.Repositories;
using MediatR;

namespace KnowledgeResourceService.Application.KnowledgeResources.Queries.GetAllKnowledgeResources;

public sealed class GetAllKnowledgeResourcesQuery : IRequest<List<KnowledgeResource>>
{
}

public sealed class GetAllKnowledgeResourcesHandler : IRequestHandler<GetAllKnowledgeResourcesQuery, List<KnowledgeResource>>
{
    private readonly IKnowledgeResourceRepository _repository;

    public GetAllKnowledgeResourcesHandler(IKnowledgeResourceRepository repository)
    {
        _repository = repository;
    }

    public Task<List<KnowledgeResource>> Handle(GetAllKnowledgeResourcesQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllKnowledgeResourcesAsync();
    }
}