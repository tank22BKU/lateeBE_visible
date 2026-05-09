using MediatR;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.CreateAssessment;

public record CreateAssessmentCommand(
    string? ModuleId,
    string Topic,
    string? Subtopic,
    string? Specialty,
    string DifficultyLevel,
    string Title,
    string? Descriptions,
    string? Goal,
    int NumQuestions,
    int? TimeLimitMinutes,
    decimal PassingScorePercentage,
    int MaxAttempts,
    string? AllowedQuestionTypes
) : IRequest<string>;

public class CreateAssessmentHandler : IRequestHandler<CreateAssessmentCommand, string>
{
    private readonly IAssessmentRepository _repo;
    public CreateAssessmentHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<string> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = new Assessment
        {
            AssessmentId = Guid.NewGuid().ToString("N"),
            ModuleId = request.ModuleId,
            Topic = request.Topic,
            Subtopic = request.Subtopic,
            Specialty = request.Specialty,
            DifficultyLevel = request.DifficultyLevel,
            Title = request.Title,
            Descriptions = request.Descriptions,
            Goal = request.Goal,
            NumQuestions = request.NumQuestions,
            TimeLimitMinutes = request.TimeLimitMinutes,
            PassingScorePercentage = request.PassingScorePercentage,
            MaxAttempts = request.MaxAttempts,
            AllowedQuestionTypes = request.AllowedQuestionTypes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(assessment);
        return assessment.AssessmentId;
    }
}