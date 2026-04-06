using MediatR;
using AssessmentService.Domain.Repositories;
using AssessmentService.Application.Queries.GetPagedAssessments; 

namespace AssessmentService.Application.Queries.GetAllAssessments;

public record GetAllAssessmentsQuery() : IRequest<List<AssessmentSummaryDto>>;

public class GetAllAssessmentsHandler : IRequestHandler<GetAllAssessmentsQuery, List<AssessmentSummaryDto>>
{
    private readonly IAssessmentRepository _repo;

    public GetAllAssessmentsHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AssessmentSummaryDto>> Handle(GetAllAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync();
        
        return items.Select(x => new AssessmentSummaryDto
        {
            AssessmentId = x.AssessmentId, 
            Title = x.Title, 
            Topic = x.Topic,
            DifficultyLevel = x.DifficultyLevel, 
            NumQuestions = x.NumQuestions,
            IsActive = x.IsActive, 
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}