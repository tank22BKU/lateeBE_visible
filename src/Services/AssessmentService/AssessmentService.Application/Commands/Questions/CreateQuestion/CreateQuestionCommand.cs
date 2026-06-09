using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.Questions.CreateQuestion;

public record CreateQuestionCommand(
    string AssessmentId,
    string QuestionType,
    string? CognitiveLevel,
    string Content,
    object? Options, 
    string? Explanation,
    decimal Points
) : IRequest<string>;

public class CreateQuestionHandler : IRequestHandler<CreateQuestionCommand, string>
{
    private readonly IAssessmentRepository _repo;
    public CreateQuestionHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<string> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdAsync(request.AssessmentId);
        if (assessment == null) throw new Exception("Assessment không tồn tại.");

        var question = new Question
        {
            Id = Guid.NewGuid().ToString("N"),
            AssessmentId = request.AssessmentId,
            Content = request.Content,
            QuestionOption = request.Options != null ? JsonSerializer.Serialize(request.Options) : null,
            QuestionType = request.QuestionType,
            CognitiveLevel = request.CognitiveLevel,
            Explanation = request.Explanation,
            Points = request.Points,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddQuestionAsync(question);
        return question.Id;
    }
}