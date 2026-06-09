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

        if (assessment == null)
            throw new Exception("Assessment không tồn tại.");

        // Get previous attempts for this user and assessment
        var previousSessions = await _repo.GetSessionsForLearnerAndAssessmentAsync(
            request.UserId, request.AssessmentId);

        int attemptCount = previousSessions.Count;

        // Check if user has exceeded max attempts
        if (attemptCount >= assessment.MaxAttempts)
            throw new Exception($"You have exceeded the maximum number of attempts ({assessment.MaxAttempts}) for this assessment.");

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var session = new AssessmentSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            AssessmentId = request.AssessmentId,
            LearnerId = request.UserId,
            AttemptNo = attemptCount + 1,
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

            var options = JsonSerializer.Deserialize<List<OptionElement>>(question.QuestionOption ?? "[]", jsonOptions)
                          ?? new List<OptionElement>();

            var correctOption = options?.FirstOrDefault(o => o.IsCorrect);
            var selectedOptionId = NormalizeSelectedOptionId(userAnswer.SelectedOptionId);

            bool isCorrect = correctOption != null &&
                            !string.IsNullOrWhiteSpace(selectedOptionId) &&
                            string.Equals(correctOption.Id?.Trim(), selectedOptionId, StringComparison.OrdinalIgnoreCase);

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
                UserChoice = JsonSerializer.Serialize(selectedOptionId),
                IsCorrect = isCorrect,
                PointsEarned = isCorrect ? question.Points : 0
            });
        }

        decimal maxPoints = assessment.Questions.Sum(q => q.Points);
        session.OverallScore = totalPointsEarned > 0 ? (totalPointsEarned) : 0;
        session.IsPassed = (session.OverallScore / maxPoints) >= (assessment.PassingScorePercentage / 100);

        await _repo.AddSessionAsync(session);

        return new SubmitResultDto(session.SessionId, session.OverallScore, session.IsPassed ?? false, correctCount);
    }

    private static string? NormalizeSelectedOptionId(string? selectedOptionId)
    {
        if (string.IsNullOrWhiteSpace(selectedOptionId)) return null;

        var trimmedValue = selectedOptionId.Trim();

        if ((trimmedValue.StartsWith("{") && trimmedValue.EndsWith("}")) ||
            (trimmedValue.StartsWith("[") && trimmedValue.EndsWith("]")) ||
            (trimmedValue.StartsWith("\"") && trimmedValue.EndsWith("\"")))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmedValue);
                return document.RootElement.ValueKind switch
                {
                    JsonValueKind.String => document.RootElement.GetString()?.Trim(),
                    JsonValueKind.Object when document.RootElement.TryGetProperty("id", out var idProperty) => idProperty.GetString()?.Trim(),
                    JsonValueKind.Object when document.RootElement.TryGetProperty("selectedOptionId", out var optionProperty) => optionProperty.GetString()?.Trim(),
                    _ => trimmedValue
                };
            }
            catch (JsonException)
            {
                return trimmedValue;
            }
        }

        return trimmedValue;
    }
}

public class OptionElement
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}