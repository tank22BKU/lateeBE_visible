using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.SubmitAssessment;

public record SubmitAssessmentCommand(
    string AssessmentId,
    string UserId,
    int DurationSeconds,
    List<UserAnswerDto> Answers
) : IRequest<SubmitResultDto>;

public record UserAnswerDto(string QuestionId, string SelectedOptionId);

public record SubmitResultDto(string AttemptId, decimal Score, bool IsPassed, int CorrectCount);

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

        var attempt = new AssessmentAttempt
        {
            AssessmentId = request.AssessmentId,
            UserId = request.UserId,
            StartTime = DateTime.UtcNow.AddSeconds(-request.DurationSeconds),
            EndTime = DateTime.UtcNow,
            Status = "Completed"
        };

        int correctCount = 0;
        decimal totalPointsEarned = 0;

        foreach (var userAnswer in request.Answers)
        {
            var question = assessment.Questions.FirstOrDefault(q => q.QuestionId == userAnswer.QuestionId);
            if (question == null) continue;

            var options = JsonSerializer.Deserialize<List<OptionElement>>(question.Options ?? "[]");
            var correctOption = options?.FirstOrDefault(o => o.IsCorrect);
            
            bool isCorrect = correctOption != null && correctOption.Id == userAnswer.SelectedOptionId;
            
            if (isCorrect)
            {
                correctCount++;
                totalPointsEarned += question.Points;
            }

            attempt.Answers.Add(new AttemptAnswer
            {
                QuestionId = question.QuestionId,
                UserChoice = userAnswer.SelectedOptionId,
                IsCorrect = isCorrect,
                PointsEarned = isCorrect ? question.Points : 0
            });
        }

        decimal maxPoints = assessment.Questions.Sum(q => q.Points);
        attempt.Score = maxPoints > 0 ? (totalPointsEarned / maxPoints) * 100 : 0;
        attempt.IsPassed = attempt.Score >= assessment.PassingScorePercentage;

        await _repo.AddAttemptAsync(attempt);

        return new SubmitResultDto(
            attempt.AttemptId, 
            attempt.Score, 
            attempt.IsPassed, 
            correctCount
        );
    }
}

public class OptionElement 
{ 
    public string Id { get; set; } = string.Empty; 
    public bool IsCorrect { get; set; } 
}