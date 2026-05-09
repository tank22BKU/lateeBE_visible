using System;

namespace KnowledgeResourceService.Domain.Entities;

public class KnowledgeResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
