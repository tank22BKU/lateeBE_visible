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
        var items = await _repo.GetByLearnerIdAsync(request.UserId);

        return items.Select(x => new EvaluationHistoryItemDto
        {
            EvaluationId = x.Id,
            PracticeSessionId = x.PracticeSessionId,
            Score = x.Score,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}

public class EvaluationHistoryItemDto
{
    public string EvaluationId { get; set; } = default!;
    public string PracticeSessionId { get; set; } = default!;
    public decimal? Score { get; set; }
    public DateTime CreatedAt { get; set; }
}