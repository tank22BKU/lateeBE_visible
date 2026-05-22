using KnowledgeResourceService.Domain.Entities;
using KnowledgeResourceService.Domain.Repositories;
using KnowledgeResourceService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeResourceService.Infrastructure.Repositories;

public class KnowledgeResourceRepository : IKnowledgeResourceRepository
{
    private readonly KnowledgeDbContext _db;

    public KnowledgeResourceRepository(KnowledgeDbContext db)
    {
        _db = db;
    }

    public Task<List<KnowledgeResource>> GetAllKnowledgeResourcesAsync()
    {
        return _db.KnowledgeResources.AsNoTracking().ToListAsync();
    }

    public Task<KnowledgeResource?> GetKnowledgeResourceByIdAsync(string id)
    {
        return _db.KnowledgeResources.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<KnowledgeResource> CreateKnowledgeResourceAsync(KnowledgeResource knowledgeResource)
    {
        knowledgeResource.Id = string.IsNullOrWhiteSpace(knowledgeResource.Id) ? Guid.NewGuid().ToString() : knowledgeResource.Id.Trim();
        knowledgeResource.CreatedAt = DateTime.UtcNow;
        knowledgeResource.UpdatedAt = DateTime.UtcNow;

        _db.KnowledgeResources.Add(knowledgeResource);
        await _db.SaveChangesAsync();
        return knowledgeResource;
    }

    public async Task<KnowledgeResource?> UpdateKnowledgeResourceAsync(KnowledgeResource knowledgeResource)
    {
        var existing = await _db.KnowledgeResources.FindAsync(knowledgeResource.Id);
        if (existing is null)
        {
            return null;
        }

        existing.Title = knowledgeResource.Title;
        existing.Content = knowledgeResource.Content;
        existing.AuthorList = knowledgeResource.AuthorList;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteKnowledgeResourceAsync(string id)
    {
        var existing = await _db.KnowledgeResources.FindAsync(id);
        if (existing is null)
        {
            return false;
        }

        _db.KnowledgeResources.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}