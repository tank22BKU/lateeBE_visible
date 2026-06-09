using MediatR;
using AssessmentService.Application.Dtos;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Queries.GetPagedAssessments;

public record GetPagedAssessmentsQuery(string? Specialty, string? DifficultyLevel, int Page, int PageSize) 
    : IRequest<PagedResult<AssessmentSummaryDto>>;

public class GetPagedAssessmentsHandler : IRequestHandler<GetPagedAssessmentsQuery, PagedResult<AssessmentSummaryDto>>
{
    private readonly IAssessmentRepository _repo;
    public GetPagedAssessmentsHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<PagedResult<AssessmentSummaryDto>> Handle(GetPagedAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetPagedAsync(request.Specialty, request.DifficultyLevel, request.Page, request.PageSize);
        var dtos = items.Select(x => new AssessmentSummaryDto
        {
            AssessmentId = x.AssessmentId, Title = x.Title, Topic = x.Topic,
            DifficultyLevel = x.DifficultyLevel, NumQuestions = x.NumQuestions,
            IsActive = x.IsActive, CreatedAt = x.CreatedAt
        }).ToList();

        return new PagedResult<AssessmentSummaryDto> { Items = dtos, Total = total, Page = request.Page, PageSize = request.PageSize };
    }
}