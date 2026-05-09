using PracticeSessionService.Application.Dtos;

namespace PracticeSessionService.Application.Queries.GetPracticeSessions;
public class GetPracticeSessionsResponse
{
    public string SessionId { get; set; } = default!;
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string ModuleId { get; set; } = "EPA_STANDARD_V1";
    public string DiscussionType { get; set; } = "Message Type";
    public string? GuidelinesId { get; set; }
    public string VpConversationLog { get; set; } = default!;
    public string AiReasoningLog { get; set; } = default!;
    public string FinalDiagnosis { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WarningDto> Warnings { get; set; } = [];
}