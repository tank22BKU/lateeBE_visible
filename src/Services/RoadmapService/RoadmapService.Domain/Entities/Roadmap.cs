namespace RoadmapService.Domain.Entities;

public class Roadmap
{
    public string RoadmapId { get; set; } = null!;

    public string LearnerId { get; set; } = null!;

    public string Content { get; set; } = "{}";

    public string? Version { get; set; }

    public DateTime CreatedAt { get; set; }
}