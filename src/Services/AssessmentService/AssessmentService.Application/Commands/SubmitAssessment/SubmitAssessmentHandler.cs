using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.SubmitAssessment;

public class SubmitAssessmentHandler : IRequestHandler<SubmitAssessmentCommand, SubmitResultDto>
{
    private readonly IAssessmentRepository _repo;

    public SubmitAssessmentHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<SubmitResultDto> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdWithQuestionsAsync(request.AssessmentId);
        if (assessment == null) throw new Exception("Assessment không tồn tại.");

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var session = new AssessmentSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            AssessmentId = request.AssessmentId,
            LearnerId = request.UserId,
            AttemptNo = 1,
            Duration = request.DurationSeconds,
            StartTime = DateTime.UtcNow.AddSeconds(-request.DurationSeconds),
            EndTime = DateTime.UtcNow,
            Status = "Completed",
            Answers = new List<AssessmentAnswer>()
        };

        int correctCount = 0;
        decimal totalPointsEarned = 0;

        foreach (var userAnswer in request.Answers)
        {
            var question = assessment.Questions.FirstOrDefault(q => q.Id == userAnswer.QuestionId);
            if (question == null) continue;

            var options = JsonSerializer.Deserialize<List<OptionElement>>(question.QuestionOption ?? "[]", jsonOptions);
    
            var correctOption = options?.FirstOrDefault(o => o.IsCorrect);

            bool isCorrect = correctOption != null && 
                            string.Equals(correctOption.Id?.Trim(), userAnswer.SelectedOptionId?.Trim(), StringComparison.OrdinalIgnoreCase);
            
            if (isCorrect)
            {
                correctCount++;
                totalPointsEarned += question.Points;
            }

            session.Answers.Add(new AssessmentAnswer
            {
                Id = Guid.NewGuid().ToString("N"),
                SessionId = session.SessionId,
                QuestionId = question.Id,
                UserChoice = userAnswer.SelectedOptionId ?? string.Empty,
                IsCorrect = isCorrect,
                PointsEarned = isCorrect ? question.Points : 0
            });
        }

        decimal maxPoints = assessment.Questions.Sum(q => q.Points);
        session.OverallScore = maxPoints > 0 ? (totalPointsEarned / maxPoints) * 100 : 0;
        session.IsPassed = session.OverallScore >= assessment.PassingScorePercentage;

        await _repo.AddSessionAsync(session);

        return new SubmitResultDto(session.SessionId, session.OverallScore, session.IsPassed ?? false, correctCount);
    }
}

public class OptionElement 
{ 
    public string Id { get; set; } = string.Empty; 
    public bool IsCorrect { get; set; } 
}