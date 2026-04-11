using MediatR;
using AssessmentService.Domain.Repositories;

namespace AssessmentService.Application.Commands.Questions.DeleteQuestion;

public record DeleteQuestionCommand(string QuestionId) : IRequest<bool>;

public class DeleteQuestionHandler : IRequestHandler<DeleteQuestionCommand, bool>
{
    private readonly IAssessmentRepository _repo;
    public DeleteQuestionHandler(IAssessmentRepository repo) { _repo = repo; }

    public async Task<bool> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _repo.GetQuestionByIdAsync(request.QuestionId);
        if (question == null) return false;

        await _repo.DeleteQuestionAsync(question);
        return true;
    }
}