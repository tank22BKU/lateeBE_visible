using PracticeSessionService.Application.Dtos;

namespace PracticeSessionService.Application.Queries.GetPracticeSessions;
public class GetPracticeSessionsResponse
{
    public string ResultId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string ClinicalCaseId { get; set; } = default!;

    public string ModuleId { get; set; } = "EPA_STANDARD_V1";

    public string VpConversationLog { get; set; } = default!;
    public string AiReasoningLog { get; set; } = default!;

    public string FinalDiagnosis { get; set; } = default!;
    public decimal OverallScore { get; set; }

    public List<WarningDto> Warnings { get; set; } = [];
}