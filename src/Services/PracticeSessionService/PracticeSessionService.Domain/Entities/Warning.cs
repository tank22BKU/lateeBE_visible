namespace PracticeSessionService.Domain.Entities;

public class Warning
{
    public string Id { get; set; } = default!;

    public string PracticeSessionId { get; set; } = default!;

    public string LearnerId { get; set; } = default!;

    public string? Label { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
