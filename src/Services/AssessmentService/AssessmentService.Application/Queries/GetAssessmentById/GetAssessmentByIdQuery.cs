using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Queries.GetAssessmentById;

public record GetAssessmentByIdQuery(string AssessmentId) : IRequest<AssessmentDetailDto?>;

public class GetAssessmentByIdHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDetailDto?>
{
    private readonly IAssessmentRepository _repo;
    public GetAssessmentByIdHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<AssessmentDetailDto?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdWithQuestionsAsync(request.AssessmentId);
        if (assessment == null) return null;

        return new AssessmentDetailDto
        {
            AssessmentId = assessment.AssessmentId, 
            Title = assessment.Title, 
            Topic = assessment.Topic,
            DifficultyLevel = assessment.DifficultyLevel, NumQuestions = assessment.NumQuestions,
            IsActive = assessment.IsActive, CreatedAt = assessment.CreatedAt, Descriptions = assessment.Descriptions,
            Goal = assessment.Goal, Specialty = assessment.Specialty, TimeLimitMinutes = assessment.TimeLimitMinutes,
            Questions = assessment.Questions.Select(q => new AssessmentQuestionDto
            {
                QuestionId = q.QuestionId, QuestionType = q.QuestionType, Content = q.Content,
                Options = string.IsNullOrEmpty(q.Options) ? null : JsonSerializer.Deserialize<object>(q.Options),
                Explanation = q.Explanation, Points = q.Points
            }).ToList()
        };
    }
}