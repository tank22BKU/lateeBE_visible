using EvaluationService.Domain.Repositories;
using MediatR;

namespace EvaluationService.Application.Commands.DeleteEvaluation;

public record DeleteEvaluationCommand(string Id) : IRequest<bool>;

public sealed class DeleteEvaluationHandler : IRequestHandler<DeleteEvaluationCommand, bool>
{
    private readonly IEvaluationRepository _repo;

    public DeleteEvaluationHandler(IEvaluationRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeleteEvaluationCommand cmd, CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(cmd.Id);
        if (existing == null)
            return false;

        await _repo.DeleteAsync(cmd.Id);
        await _repo.SaveChangesAsync();
        return true;
    }
}
