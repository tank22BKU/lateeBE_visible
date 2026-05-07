using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;
using System.Text.Json;
using MediatR;

namespace PracticeSessionService.Application.Queries.SavePracticeSessions;

public class SavePracticeSessionsHandler 
    : IRequestHandler<SavePracticeSessionsRequest, SavePracticeSessionsResponse>
{
    private readonly IPracticeSessionRepository _repo;

    public SavePracticeSessionsHandler(IPracticeSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SavePracticeSessionsResponse> Handle(
        SavePracticeSessionsRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _repo.GetSessionByIdAsync(request.SessionId);
        if (session == null)
        {
            throw new Exception("Practice session not found");
        }

        session.FinalDiagnosis = request.FinalDiagnosis ?? session.FinalDiagnosis;
        session.VpConversationLog = request.VpConversationLog != null
            ? JsonSerializer.Serialize(request.VpConversationLog)
            : session.VpConversationLog;
        session.AiReasoningLog = request.AiReasoningLog != null
            ? JsonSerializer.Serialize(request.AiReasoningLog)
            : session.AiReasoningLog;
        session.ModuleId = request.ModuleId ?? session.ModuleId;
        session.DiscussionType = request.DiscussionType ?? session.DiscussionType;
        session.GuidelinesId = request.GuidelinesId ?? session.GuidelinesId;
        session.EndTime = DateTime.UtcNow;
        session.Status = "Completed";

        var warnings = request.Warnings.Select(w => new Warning
        {
            Id = w.WarningId,
            PracticeSessionId = request.SessionId,
            LearnerId = request.LearnerId,
            Label = w.Label,
            Description = w.Description,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _repo.UpdateSessionAsync(session);
        await _repo.AddWarningsAsync(warnings);
        await _repo.SaveChangesAsync();

        return new SavePracticeSessionsResponse(session.Id);
    }
}