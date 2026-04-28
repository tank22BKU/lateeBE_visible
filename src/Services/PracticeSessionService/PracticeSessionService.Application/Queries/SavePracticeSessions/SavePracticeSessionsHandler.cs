using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;
using System.Text.Json;
using MediatR;

namespace PracticeSessionService.Application.Queries.SavePracticeSessions;

public class SubmitEvaluationHandler 
    : IRequestHandler<SavePracticeSessionsRequest, SavePracticeSessionsResponse>
{
    private readonly IPracticeSessionRepository _repo;

    public SubmitEvaluationHandler(IPracticeSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SavePracticeSessionsResponse> Handle(
        SavePracticeSessionsRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PracticeSessionResult
        {
            ResultId = request.ResultId,
            SessionId = request.SessionId,
            UserId = request.UserId,
            ClinicalCaseId = request.ClinicalCaseId,
            ModuleId = request.ModuleId,

            VpConversationLog = JsonSerializer.Serialize(request.VpConversationLog),

            AiReasoningLog = JsonSerializer.Serialize(request.AiReasoningLog),

            FinalDiagnosis = request.FinalDiagnosis,

            OverallScore = request.OverallScore,

            Warnings = request.Warnings?.Select(w => new EvaluationWarning
            {
                WarningId = w.WarningId,
                ResultId = request.ResultId,
                Label = w.Label,
                Description = w.Description
            }).ToList() ?? new List<EvaluationWarning>()
        };

        string result = await _repo.AddAsync(entity);

        return new SavePracticeSessionsResponse(result);
    }
}