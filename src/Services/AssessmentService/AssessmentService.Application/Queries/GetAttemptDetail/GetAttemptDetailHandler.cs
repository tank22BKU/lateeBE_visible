using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Queries.GetAttemptDetails;

public record GetAttemptDetailQuery(string AttemptId) : IRequest<GetAttemptDetailDto?>;

public class GetAttemptDetailHandler : IRequestHandler<GetAttemptDetailQuery, GetAttemptDetailDto?>
{
    private readonly IAssessmentRepository _repo;

    public GetAttemptDetailHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<GetAttemptDetailDto?> Handle(GetAttemptDetailQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _repo.GetAttemptWithAnswersAsync(request.AttemptId);
        if (attempt == null) return null;

        var assessment = await _repo.GetByIdWithQuestionsAsync(attempt.AssessmentId);
        if (assessment == null) return null;

        return new GetAttemptDetailDto
        {
            AttemptId = attempt.AttemptId,
            Score = attempt.Score,
            IsPassed = attempt.IsPassed,
            CorrectCount = attempt.Answers.Count(x => x.IsCorrect),
            Questions = assessment.Questions.Select(q => 
            {
                var userAns = attempt.Answers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                var options = JsonSerializer.Deserialize<List<OptionResultDto>>(q.Options ?? "[]", new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<OptionResultDto>();

                return new QuestionResultDto
                {
                    QuestionId = q.QuestionId,
                    Content = q.Content,
                    Explanation = q.Explanation,
                    UserAnswerId = userAns?.UserChoice,
                    CorrectAnswerId = options.FirstOrDefault(o => o.IsCorrect)?.Id,
                    IsCorrect = userAns?.IsCorrect ?? false,
                    Options = options
                };
            }).ToList()
        };
    }
}