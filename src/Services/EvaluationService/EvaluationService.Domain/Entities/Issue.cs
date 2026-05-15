namespace EvaluationService.Domain.Entities;

public class Issue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string PracticeSessionId { get; set; } = string.Empty;

    public string LearnerId { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ItemType { get; set; } = string.Empty;

    public DateTime? EditDeadline { get; set; }

    public string Status { get; set; } = "Open";

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}