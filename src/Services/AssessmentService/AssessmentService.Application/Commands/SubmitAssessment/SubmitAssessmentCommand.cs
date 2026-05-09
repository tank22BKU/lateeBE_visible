using MediatR;

namespace AssessmentService.Application.Commands.SubmitAssessment;

public record SubmitAssessmentCommand(
    string AssessmentId,
    string UserId,
    int DurationSeconds,
    List<UserAnswerDto> Answers
) : IRequest<SubmitResultDto>;

public record UserAnswerDto(string QuestionId, string SelectedOptionId);

public record SubmitResultDto(string AttemptId, decimal Score, bool IsPassed, int CorrectCount);