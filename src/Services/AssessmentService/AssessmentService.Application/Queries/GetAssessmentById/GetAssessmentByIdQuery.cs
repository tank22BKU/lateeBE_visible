using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AssessmentService.Application.Queries.GetAssessmentById;

public record GetAssessmentByIdQuery(string AssessmentId) : IRequest<AssessmentDetailDto?>;

public class GetAssessmentByIdHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDetailDto?>
{
    private readonly IAssessmentRepository _repo;
    private readonly ILogger<GetAssessmentByIdHandler> _logger;
    public GetAssessmentByIdHandler(IAssessmentRepository repo, ILogger<GetAssessmentByIdHandler> logger) { _repo = repo; _logger = logger; }

    public async Task<AssessmentDetailDto?> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdWithQuestionsAsync(request.AssessmentId);
        if (assessment == null) return null;
        
        decimal maxScore = assessment.Questions.Count > 0
            ? assessment.Questions.Sum(x => x.Points)
            : assessment.NumQuestions;

        return new AssessmentDetailDto
        {
            AssessmentId = assessment.AssessmentId,
            Title = assessment.Title,
            Topic = assessment.Topic,
            Subtopic = assessment.Subtopic ?? " ",
            DifficultyLevel = assessment.DifficultyLevel,
            NumQuestions = assessment.NumQuestions,
            IsActive = assessment.IsActive,
            CreatedAt = assessment.CreatedAt,
            Descriptions = assessment.Descriptions ?? " ",
            PassingScorePercentage = assessment.PassingScorePercentage,
            MaxAttempts = assessment.MaxAttempts,
            MaxScore = maxScore,
            TimesPracticed = 0, ///// do not care because do not know UserId
            Goal = assessment.Goal,
            Specialty = assessment.Specialty,
            TimeLimitMinutes = assessment.TimeLimitMinutes,
            Questions = assessment.Questions.Select(q => new AssessmentQuestionDto
            {
                Id = q.Id,
                Question = q.Content,
                QuestionOption = DeserializeQuestionOptionSafe(q.QuestionOption),
                QuestionType = q.QuestionType,
                Explanation = q.Explanation,
                Points = q.Points
            }).ToList()
        };
    }

    private object? DeserializeQuestionOptionSafe(string? questionOption)
    {
        if (string.IsNullOrEmpty(questionOption)) return null;

        try
        {
            return JsonSerializer.Deserialize<object>(questionOption);
        }
        catch (JsonException jex)
        {
            _logger.LogWarning(jex, "Failed to deserialize QuestionOption, returning raw string.");
            return questionOption;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error deserializing QuestionOption, returning raw string.");
            return questionOption;
        }
    }
}