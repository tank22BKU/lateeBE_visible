using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Domain.Repositories;
using MediatR;

namespace KnowledgeResourceService.Application.KnowledgeResources.Commands.CreateKnowledgeResource;

public sealed class CreateKnowledgeResourceCommand : IRequest<KnowledgeResource>
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    
    public string? AuthorList { get; set; }
}

public sealed class CreateKnowledgeResourceHandler : IRequestHandler<CreateKnowledgeResourceCommand, KnowledgeResource>
{
    private readonly IKnowledgeResourceRepository _repository;

    public CreateKnowledgeResourceHandler(IKnowledgeResourceRepository repository)
    {
        _repository = repository;
    }

    public Task<KnowledgeResource> Handle(CreateKnowledgeResourceCommand request, CancellationToken cancellationToken)
    {
        var entity = new KnowledgeResource
        {
            Id = request.Id ?? string.Empty,
            Title = request.Title,
            Content = request.Content,
            AuthorList = request.AuthorList
        };

        return _repository.CreateKnowledgeResourceAsync(entity);
    }
}