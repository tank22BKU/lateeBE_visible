namespace PracticeSessionService.Application.Dtos;

public class WarningDto
{
    public string WarningId { get; set; } = default!;

    public string PracticeSessionId { get; set; } = default!;

    public string LearnerId { get; set; } = default!;

    public string Label { get; set; } = default!;

    public string Description { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
