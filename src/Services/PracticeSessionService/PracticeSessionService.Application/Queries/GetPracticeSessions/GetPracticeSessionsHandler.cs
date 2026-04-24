using MediatR;
using PracticeSessionService.Domain.Repositories;
using PracticeSessionService.Application.Dtos;

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
        CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.ResultId);

        if (entity == null)
        {
            throw new Exception("Practice session not found");
        }

        return new GetPracticeSessionsResponse
        {
            ResultId = entity.ResultId,
            UserId = entity.UserId,
            ClinicalCaseId = entity.ClinicalCaseId,

            ModuleId = entity.ModuleId,

            VpConversationLog = entity.VpConversationLog ?? "",

            AiReasoningLog = entity.AiReasoningLog ?? "",

            FinalDiagnosis = entity.FinalDiagnosis ?? "",

            OverallScore = entity.OverallScore,

            Warnings = entity.Warnings.Select(w => new WarningDto
            {
                WarningId = w.WarningId,
                Label = w.Label ?? "",
                Description = w.Description ?? ""
            }).ToList()
        };
    }
}