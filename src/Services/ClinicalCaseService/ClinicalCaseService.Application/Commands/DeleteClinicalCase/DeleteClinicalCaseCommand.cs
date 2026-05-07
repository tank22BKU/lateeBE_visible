using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Commands.DeleteClinicalCase;

public record DeleteClinicalCaseCommand(string CaseId) : IRequest<bool>;

public class DeleteClinicalCaseHandler : IRequestHandler<DeleteClinicalCaseCommand, bool>
{
	private readonly IClinicalCaseRepository _repo;

	public DeleteClinicalCaseHandler(IClinicalCaseRepository repo)
	{
		_repo = repo;
	}

	public async Task<bool> Handle(DeleteClinicalCaseCommand request, CancellationToken cancellationToken)
	{
		var clinicalCase = await _repo.GetByIdAsync(request.CaseId);

		if (clinicalCase == null)
		{
			return false;
		}

		await _repo.DeleteAsync(clinicalCase);
		return true;
	}
}