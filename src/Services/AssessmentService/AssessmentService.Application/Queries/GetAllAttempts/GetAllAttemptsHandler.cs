using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Repositories;
using AssessmentService.Application.Queries.GetAttemptDetails;

namespace AssessmentService.Application.Queries.GetAllAttempts;

public record GetAllAttemptsQuery(string AssessmentId, string LearnerId) : IRequest<List<GetAttemptDetailDto>>;

public class GetAllAttemptsHandler : IRequestHandler<GetAllAttemptsQuery, List<GetAttemptDetailDto>>
{
    private readonly IAssessmentRepository _repo;

    public GetAllAttemptsHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<GetAttemptDetailDto>> Handle(GetAllAttemptsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repo.GetSessionsForLearnerAndAssessmentAsync(request.LearnerId, request.AssessmentId);

        var results = new List<GetAttemptDetailDto>();

        foreach (var session in sessions)
        {
            var dto = new GetAttemptDetailDto
            {
                AttemptId = session.SessionId,
                Score = session.OverallScore,
                IsPassed = session.IsPassed ?? false,
                CorrectCount = session.Answers.Count(x => x.IsCorrect),
                Questions = await BuildQuestionResultsAsync(session, cancellationToken)
            };

            results.Add(dto);
        }

        return results;
    }

    private async Task<List<QuestionResultDto>> BuildQuestionResultsAsync(
        AssessmentService.Domain.Entities.AssessmentSession session,
        CancellationToken cancellationToken)
    {
        var results = new List<QuestionResultDto>();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var answer in session.Answers)
        {
            var question = await _repo.GetQuestionByIdAsync(answer.QuestionId);
            if (question == null) continue;

            var options = JsonSerializer.Deserialize<List<OptionResultDto>>(question.QuestionOption ?? "[]", jsonOptions)
                          ?? new List<OptionResultDto>();

            results.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                Content = question.Content,
                UserAnswerId = !string.IsNullOrEmpty(answer.UserChoice) ? JsonSerializer.Deserialize<string?>(answer.UserChoice, jsonOptions) : null,
                CorrectAnswerId = options.FirstOrDefault(o => o.IsCorrect)?.Id,
                IsCorrect = answer.IsCorrect,
                Explanation = question.Explanation,
                Options = options
            });
        }

        return results;
    }
}
