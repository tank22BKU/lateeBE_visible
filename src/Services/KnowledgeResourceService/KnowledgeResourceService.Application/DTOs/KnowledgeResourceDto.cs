using System;

namespace KnowledgeResourceService.Application.DTOs;

public class KnowledgeResourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? AuthorList { get; set; }
}
