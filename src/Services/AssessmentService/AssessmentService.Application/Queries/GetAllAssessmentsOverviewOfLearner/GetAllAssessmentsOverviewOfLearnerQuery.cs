using MediatR;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Queries.GetAllAssessmentsOverviewOfLearner;

public record GetAllAssessmentsOverviewOfLearnerQuery(string LearnerId)
    : IRequest<List<AssessmentDataDto>>;

public class GetAllAssessmentsOverviewOfLearnerHandler
    : IRequestHandler<GetAllAssessmentsOverviewOfLearnerQuery, List<AssessmentDataDto>>
{
    private readonly IAssessmentRepository _repo;

    public GetAllAssessmentsOverviewOfLearnerHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AssessmentDataDto>> Handle(
        GetAllAssessmentsOverviewOfLearnerQuery request,
        CancellationToken cancellationToken)
    {
        var learnerId = request.LearnerId?.Trim();
        if (string.IsNullOrWhiteSpace(learnerId))
        {
            return new List<AssessmentDataDto>();
        }
        
        var allAssessments = await _repo.GetAllAsync();
        
        Console.WriteLine("AllAssessments: " + allAssessments.Count);
        
        if (allAssessments.Count == 0)
        {
            return new List<AssessmentDataDto>();
        }
        
        var sessions = await _repo.GetAllAttemptsOverviewOfLearner(learnerId);
        
        Console.WriteLine("LearnerId: " + learnerId + " - Sessions: " + sessions.Count + " - " + sessions);
        
        var sessionsByAssessment = sessions
            .GroupBy(x => x.AssessmentId)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        Console.WriteLine("SessionsByAssessment: " + sessionsByAssessment);
        
        var results = new List<AssessmentDataDto>();
        
        foreach (var assessment in allAssessments)
        {
            var maxScore = assessment.Questions.Count > 0
                ? assessment.Questions.Sum(x => x.Points)
                : assessment.NumQuestions;

            // Nếu learner có attempts cho assessment này, map chúng thành DTO
            var attempts = new List<AttemptItemDto>();
            int timesPracticed = 0;

            if (sessionsByAssessment.TryGetValue(assessment.AssessmentId, out var sessionsForAssessment))
            {
                timesPracticed = sessionsForAssessment.Count;
                attempts = sessionsForAssessment
                    .OrderBy(x => x.AttemptNo)
                    .Select(x => new AttemptItemDto
                    {
                        AttempId = x.SessionId,
                        LearnerId = x.LearnerId,
                        AttemptNo = x.AttemptNo,
                        Duration = x.Duration ?? 0,
                        Score = x.OverallScore,
                        IsPassed = x.IsPassed ?? false
                    })
                    .ToList();
            }
            
            results.Add(new AssessmentDataDto
            {
                AssessmentId = assessment.AssessmentId,
                CreatedAt = assessment.CreatedAt,
                CreatorId = string.Empty,
                ModuleId = !String.IsNullOrEmpty(assessment.ModuleId) ? assessment.ModuleId : string.Empty,
                Specialty = !String.IsNullOrEmpty(assessment.Specialty) ? assessment.Specialty : string.Empty,
                Topic = assessment.Topic,
                Subtopic = !String.IsNullOrEmpty(assessment.Subtopic) ? assessment.Subtopic : string.Empty,
                MaxScore = maxScore,
                Descriptions = !String.IsNullOrEmpty(assessment.Descriptions) ? assessment.Descriptions : string.Empty,
                DifficultyLevel = assessment.DifficultyLevel,
                Title = assessment.Title,
                Goal = !String.IsNullOrEmpty(assessment.Goal) ? assessment.Goal : string.Empty,
                NumQuestions = assessment.NumQuestions,
                TimeLimitMinutes = assessment.TimeLimitMinutes ?? 0,
                TimesPracticed = timesPracticed,
                MaxAttempts = assessment.MaxAttempts,
                PassingScorePercentage = assessment.PassingScorePercentage,
                IsActive = assessment.IsActive,
                ListAttempts = attempts
            });
        }

        return results
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }
}