namespace PracticeSessionService.Domain.Entities;

public class PracticeSession
{
    public string Id { get; set; } = default!;

    public string LearnerId { get; set; } = default!;

    public string PatientId { get; set; } = default!;

    public string? FinalDiagnosis { get; set; }

    public string? AiReasoningLog { get; set; }

    public string? VpConversationLog { get; set; }

    public string? ModuleId { get; set; }

    public string? DiscussionType { get; set; }

    public string? GuidelinesId { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    public string Status { get; set; } = "Practicing";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
