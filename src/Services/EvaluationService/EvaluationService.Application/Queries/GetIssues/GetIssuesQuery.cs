using MediatR;
using EvaluationService.Application.Dtos;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Queries.GetIssues;

public record GetIssuesQuery(string PracticeSessionId, string LearnerId)
    : IRequest<IssueListResponseDto>;

public sealed class GetIssuesHandler : IRequestHandler<GetIssuesQuery, IssueListResponseDto>
{
    private readonly IEvaluationRepository _repo;

    public GetIssuesHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<IssueListResponseDto> Handle(GetIssuesQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.PracticeSessionId))
            throw new ArgumentException("PracticeSessionId is required.");
        if (string.IsNullOrWhiteSpace(query.LearnerId))
            throw new ArgumentException("LearnerId is required.");

        var items = await _repo.GetIssuesAsync(query.PracticeSessionId, query.LearnerId);

        return new IssueListResponseDto
        {
            Items = items.Select(x => new IssueItemDto
            {
                IssueId = x.IssueId,
                LearnerId = x.LearnerId,
                LearnerName = x.LearnerName,
                CreatedAt = x.CreatedAt,
                Label = x.Label,
                Description = x.Description,
                Status = x.Status,
                ExpertFeedback = x.ExpertFeedback == null ? null : new IssueExpertFeedbackDto
                {
                    ExpertId = x.ExpertFeedback.ExpertId,
                    ExpertName = x.ExpertFeedback.ExpertName,
                    Feedback = x.ExpertFeedback.Feedback
                }
            }).ToList()
        };
    }
}
