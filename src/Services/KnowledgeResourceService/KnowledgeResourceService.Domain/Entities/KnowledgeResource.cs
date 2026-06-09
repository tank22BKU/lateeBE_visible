using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeResourceService.Domain.Entities;

public class KnowledgeResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    [Column("authorlist")]
    public string? AuthorList { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}