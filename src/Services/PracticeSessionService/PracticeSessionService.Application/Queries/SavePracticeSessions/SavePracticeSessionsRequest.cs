using MediatR;
using PracticeSessionService.Application.Dtos;

namespace PracticeSessionService.Application.Queries.SavePracticeSessions;

public class SavePracticeSessionsRequest : IRequest<SavePracticeSessionsResponse>
{
    public string SessionId { get; set; } = default!;
    public string LearnerId { get; set; } = default!;
    public string? FinalDiagnosis { get; set; }
    public object? VpConversationLog { get; set; }
    public object? AiReasoningLog { get; set; }
    public string? ModuleId { get; set; }
    public string? DiscussionType { get; set; }
    public string? GuidelinesId { get; set; }
    public List<WarningDto> Warnings { get; set; } = [];
}
