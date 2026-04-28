using MediatR;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Queries.GetHistory;

public record GetUserHistoryQuery(string UserId) : IRequest<List<EvaluationHistoryItemDto>>;

public class GetUserHistoryHandler : IRequestHandler<GetUserHistoryQuery, List<EvaluationHistoryItemDto>>
{
    private readonly IEvaluationRepository _repo;

    public GetUserHistoryHandler(IEvaluationRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<EvaluationHistoryItemDto>> Handle(GetUserHistoryQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetByUserIdAsync(request.UserId);

        return items.Select(x => new EvaluationHistoryItemDto
        {
            ResultId = x.ResultId,
            SessionId = x.SessionId,
            ClinicalCaseId = x.ClinicalCaseId,
            FinalDiagnosis = x.FinalDiagnosis,
            OverallScore = x.OverallScore,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}

public class EvaluationHistoryItemDto
{
    public string ResultId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string ClinicalCaseId { get; set; } = default!;
    public string? FinalDiagnosis { get; set; }
    public decimal OverallScore { get; set; }
    public DateTime CreatedAt { get; set; }
}