using KnowledgeResourceService.Domain.Entities;

namespace KnowledgeResourceService.Domain.Repositories;

public interface IKnowledgeResourceRepository
{
    Task<List<KnowledgeResource>> GetAllKnowledgeResourcesAsync();

    Task<KnowledgeResource?> GetKnowledgeResourceByIdAsync(string id);

    Task<KnowledgeResource> CreateKnowledgeResourceAsync(KnowledgeResource knowledgeResource);

    Task<KnowledgeResource?> UpdateKnowledgeResourceAsync(KnowledgeResource knowledgeResource);

    Task<bool> DeleteKnowledgeResourceAsync(string id);
}