using MediatR;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Queries.GetHistory;

public record GetUserHistoryQuery(string UserId) : IRequest<List<EvaluationHistoryItemDto>>;

public sealed class GetUserHistoryHandler
    : IRequestHandler<GetUserHistoryQuery, List<EvaluationHistoryItemDto>>
{
    private readonly IEvaluationRepository _repo;

    public GetUserHistoryHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<List<EvaluationHistoryItemDto>> Handle(
        GetUserHistoryQuery query, CancellationToken ct)
    {
        var items = await _repo.GetByLearnerIdAsync(query.UserId);
        return items.Select(x => new EvaluationHistoryItemDto
        {
            EvaluationId      = x.Id,
            PracticeSessionId = x.PracticeSessionId,
            Score             = x.Score,
            EntrustmentLevel  = x.EntrustmentLevel,
            RubricVersion     = x.RubricVersion,
            CreatedAt         = x.CreatedAt
        }).ToList();
    }
}

public class EvaluationHistoryItemDto
{
    public string    EvaluationId      { get; set; } = default!;
    public string    PracticeSessionId { get; set; } = default!;
    public decimal?  Score             { get; set; }
    public int?      EntrustmentLevel  { get; set; }
    public string?   RubricVersion     { get; set; }
    public DateTime  CreatedAt         { get; set; }
}