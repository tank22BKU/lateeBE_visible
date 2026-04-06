using MediatR;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.UpdateAssessment;

public record UpdateAssessmentCommand(
    string AssessmentId, string Title, string? Descriptions, string? Goal, int? TimeLimitMinutes, bool IsActive
) : IRequest<bool>;

public class UpdateAssessmentHandler : IRequestHandler<UpdateAssessmentCommand, bool>
{
    private readonly IAssessmentRepository _repo;
    public UpdateAssessmentHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<bool> Handle(UpdateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdAsync(request.AssessmentId);
        if (assessment == null) return false;

        assessment.Title = request.Title; assessment.Descriptions = request.Descriptions;
        assessment.Goal = request.Goal; assessment.TimeLimitMinutes = request.TimeLimitMinutes;
        assessment.IsActive = request.IsActive; assessment.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(assessment);
        return true;
    }
}