using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.Questions.UpdateQuestion;

public record UpdateQuestionCommand(
    string QuestionId,
    string QuestionType,
    string? CognitiveLevel,
    string Content,
    object? Options, 
    string? Explanation,
    decimal Points
) : IRequest<bool>;

public class UpdateQuestionHandler : IRequestHandler<UpdateQuestionCommand, bool>
{
    private readonly IAssessmentRepository _repo;
    public UpdateQuestionHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<bool> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _repo.GetQuestionByIdAsync(request.QuestionId);
        if (question == null) return false;

        question.QuestionType = request.QuestionType;
        question.CognitiveLevel = request.CognitiveLevel;
        question.Content = request.Content;
        question.QuestionOption = request.Options != null ? JsonSerializer.Serialize(request.Options) : null;
        question.Explanation = request.Explanation;
        question.Points = request.Points;
        question.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateQuestionAsync(question);
        return true;
    }
}