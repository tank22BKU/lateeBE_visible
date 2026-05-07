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
        var session = await _repo.GetSessionWithAnswersAsync(request.AttemptId);
        if (session == null) return null;

        return new GetAttemptDetailDto
        {
            AttemptId = session.SessionId,
            Score = session.OverallScore,
            IsPassed = session.IsPassed ?? false,
            CorrectCount = session.Answers.Count(x => x.IsCorrect),
            Questions = await BuildQuestionResultsAsync(session, cancellationToken)
        };
    }

    private async Task<List<QuestionResultDto>> BuildQuestionResultsAsync(
        AssessmentService.Domain.Entities.AssessmentSession session,
        CancellationToken cancellationToken)
    {
        var results = new List<QuestionResultDto>();

        foreach (var answer in session.Answers)
        {
            var question = await _repo.GetQuestionByIdAsync(answer.QuestionId);
            if (question == null) continue;

            var options = JsonSerializer.Deserialize<List<OptionResultDto>>(question.QuestionOption ?? "[]", new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<OptionResultDto>();

            results.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                Content = question.Content,
                Explanation = question.Explanation,
                UserAnswerId = answer.UserChoice,
                CorrectAnswerId = options.FirstOrDefault(o => o.IsCorrect)?.Id,
                IsCorrect = answer.IsCorrect,
                Options = options
            });
        }

        return results;
    }
}