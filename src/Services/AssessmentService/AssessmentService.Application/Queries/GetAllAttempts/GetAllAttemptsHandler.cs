using MediatR;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Queries.GetAllAttempts;

public record GetAllAttemptsQuery(string AssessmentId, string LearnerId) : IRequest<List<AssessmentAttemptOverview>>;

public class GetAllAttemptsHandler : IRequestHandler<GetAllAttemptsQuery, List<AssessmentAttemptOverview>>
{
    private readonly IAssessmentRepository _repo;

    public GetAllAttemptsHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AssessmentAttemptOverview>> Handle(GetAllAttemptsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repo.GetSessionsForLearnerAndAssessmentAsync(request.LearnerId, request.AssessmentId);
        var assessment = await _repo.GetByIdAsync(request.AssessmentId);
        
        var passingScorePercentage = assessment?.PassingScorePercentage ?? 0m;
        var results = new List<AssessmentAttemptOverview>();
        
        if (sessions.Count <= 0) return results;
        
        foreach (var session in sessions)
        {
            var dto = new AssessmentAttemptOverview
            {
                AttemptId = session.SessionId,
                Score = session.OverallScore,
                IsPassed = session.IsPassed ?? false,
                CorrectCount = session.Answers.Count(x => x.IsCorrect),
                Duration = session.Duration ?? 0,
                PassingScorePercentage = passingScorePercentage
            };

            results.Add(dto);
        }

        return results;
    }
}
