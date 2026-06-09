using MediatR;
using PracticeSessionService.Application.Dtos;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Queries.GetPracticeSessions;

public class GetPracticeSessionHandler
    : IRequestHandler<GetPracticeSessionsRequest, GetPracticeSessionsResponse>
{
    private readonly IPracticeSessionRepository _repo;

    public GetPracticeSessionHandler(IPracticeSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GetPracticeSessionsResponse> Handle(
        GetPracticeSessionsRequest request,
        CancellationToken cancellationToken
    )
    {
        var session = await _repo.GetSessionByIdAsync(request.SessionId);

        if (session == null)
        {
            throw new Exception("Practice session not found");
        }

        var warnings = await _repo.GetWarningsBySessionIdAsync(session.Id);

        return new GetPracticeSessionsResponse
        {
            SessionId = session.Id,
            LearnerId = session.LearnerId,
            PatientId = session.PatientId,
            ModuleId = session.ModuleId ?? "EPA_STANDARD_V1",
            DiscussionType = session.DiscussionType ?? "Message Type",
            GuidelinesId = session.GuidelinesId,
            FinalDiagnosis = session.FinalDiagnosis ?? "",
            VpConversationLog = session.VpConversationLog ?? "",
            AiReasoningLog = session.AiReasoningLog ?? "",
            Status = session.Status,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            CreatedAt = session.CreatedAt,
            Warnings = warnings
                .Select(w => new WarningDto
                {
                    WarningId = w.Id,
                    PracticeSessionId = w.PracticeSessionId,
                    LearnerId = w.LearnerId,
                    Label = w.Label ?? "",
                    Description = w.Description ?? "",
                    CreatedAt = w.CreatedAt,
                })
                .ToList(),
        };
    }
}
