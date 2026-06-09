using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Domain.Repositories;
using MediatR;

namespace KnowledgeResourceService.Application.KnowledgeResources.Commands.UpdateKnowledgeResource;

public sealed class UpdateKnowledgeResourceCommand : IRequest<KnowledgeResource?>
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AuthorList { get; set; }
}

public sealed class UpdateKnowledgeResourceHandler : IRequestHandler<UpdateKnowledgeResourceCommand, KnowledgeResource?>
{
    private readonly IKnowledgeResourceRepository _repository;

    public UpdateKnowledgeResourceHandler(IKnowledgeResourceRepository repository)
    {
        _repository = repository;
    }

    public Task<KnowledgeResource?> Handle(UpdateKnowledgeResourceCommand request, CancellationToken cancellationToken)
    {
        var entity = new KnowledgeResource
        {
            Id = request.Id,
            Title = request.Title,
            Content = request.Content,
            AuthorList = request.AuthorList
        };

        return _repository.UpdateKnowledgeResourceAsync(entity);
    }
}