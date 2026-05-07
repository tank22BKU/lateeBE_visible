using ClinicalCaseService.Application.Queries.GetClinicalCases;
using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Queries.GetClinicalCaseById;

public class GetClinicalCaseByIdHandler : IRequestHandler<GetClinicalCaseByIdQuery, ClinicalCaseDto?>
{
	private readonly IClinicalCaseRepository _repo;

	public GetClinicalCaseByIdHandler(IClinicalCaseRepository repo)
	{
		_repo = repo;
	}

	public async Task<ClinicalCaseDto?> Handle(GetClinicalCaseByIdQuery request, CancellationToken cancellationToken)
	{
		var clinicalCase = await _repo.GetByIdAsync(request.CaseId);

		if (clinicalCase == null)
		{
			return null;
		}

		return new ClinicalCaseDto
		{
			CaseId = clinicalCase.CaseId,
			Title = clinicalCase.Title,
			Description = clinicalCase.Description,
			CaseType = clinicalCase.CaseType,
			Status = clinicalCase.Status,
			Pe = clinicalCase.Pe,
			Symptom = clinicalCase.Symptom,
			MedicalHistory = clinicalCase.MedicalHistory,
			CreatedBy = clinicalCase.CreatedBy,
			EccId = clinicalCase.EccId,
			CreatedAt = clinicalCase.CreatedAt,
			UpdatedAt = clinicalCase.UpdatedAt
		};
	}
}