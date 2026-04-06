using MediatR;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.DeleteAssessment;

public record DeleteAssessmentCommand(string AssessmentId) : IRequest<bool>;

public class DeleteAssessmentHandler : IRequestHandler<DeleteAssessmentCommand, bool>
{
    private readonly IAssessmentRepository _repo;
    public DeleteAssessmentHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<bool> Handle(DeleteAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdAsync(request.AssessmentId);
        if (assessment == null) return false;

        await _repo.DeleteAsync(assessment);
        return true;
    }
}