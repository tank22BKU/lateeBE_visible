using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Commands.UpdateClinicalCase;

public record UpdateClinicalCaseCommand(
	string CaseId,
	string Title,
	string? Description,
	string? CaseType,
	string? Status,
	string? Pe,
	string? Symptom,
	string? MedicalHistory,
	string CreatedBy,
	string EccId
) : IRequest<bool>;

public class UpdateClinicalCaseHandler : IRequestHandler<UpdateClinicalCaseCommand, bool>
{
	private readonly IClinicalCaseRepository _repo;

	public UpdateClinicalCaseHandler(IClinicalCaseRepository repo)
	{
		_repo = repo;
	}

	public async Task<bool> Handle(UpdateClinicalCaseCommand request, CancellationToken cancellationToken)
	{
		var clinicalCase = await _repo.GetByIdAsync(request.CaseId);

		if (clinicalCase == null)
		{
			return false;
		}

		clinicalCase.Title = request.Title;
		clinicalCase.Description = request.Description;
		clinicalCase.CaseType = request.CaseType;
		clinicalCase.Status = request.Status;
		clinicalCase.Pe = request.Pe;
		clinicalCase.Symptom = request.Symptom;
		clinicalCase.MedicalHistory = request.MedicalHistory;
		clinicalCase.CreatedBy = request.CreatedBy;
		clinicalCase.EccId = request.EccId;
		clinicalCase.UpdatedAt = DateTime.UtcNow;

		await _repo.UpdateAsync(clinicalCase);
		return true;
	}
}