using MediatR;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Commands.DeleteEvaluation;

public record DeleteEvaluationCommand(string ResultId) : IRequest<bool>;

public class DeleteEvaluationHandler : IRequestHandler<DeleteEvaluationCommand, bool>
{
    private readonly IEvaluationRepository _repo;

    public DeleteEvaluationHandler(IEvaluationRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteEvaluationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.ResultId);
        if (entity == null)
        {
            return false;
        }

        await _repo.DeleteAsync(request.ResultId);
        await _repo.SaveChangesAsync();
        return true;
    }
}