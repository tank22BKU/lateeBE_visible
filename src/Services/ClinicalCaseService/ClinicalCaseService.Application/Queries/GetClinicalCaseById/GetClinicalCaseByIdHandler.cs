using ClinicalCaseService.Application.Queries.GetClinicalCases;
using ClinicalCaseService.Domain.Entities;
using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Queries.GetClinicalCaseById;

public class GetClinicalCaseByIdHandler
    : IRequestHandler<GetClinicalCaseByIdQuery, ClinicalCaseDto?>
{
    private readonly IClinicalCaseRepository _repo;

    public GetClinicalCaseByIdHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<ClinicalCaseDto?> Handle(
        GetClinicalCaseByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var clinicalCase = await _repo.GetByIdAsync(request.CaseId);

        if (clinicalCase == null)
        {
            return null;
        }

        var createdByName = await _repo.GetExpertNameAsync(clinicalCase.CreatedBy);
        var labs = await _repo.GetLabsByCaseIdAsync(clinicalCase.CaseId);
        var radiology = await _repo.GetRadiologyByCaseIdAsync(clinicalCase.CaseId);
        var virtualPatients = await _repo.GetVirtualPatientsByCaseIdAsync(clinicalCase.CaseId);
        var stats =
            await _repo.GetStatsByCaseIdAsync(clinicalCase.CaseId) ?? new ClinicalCaseStats();

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
            CreatedByName = createdByName,
            EccId = clinicalCase.EccId,
            CreatedAt = clinicalCase.CreatedAt,
            UpdatedAt = clinicalCase.UpdatedAt,
            Labs = labs,
            Radiology = radiology,
            VirtualPatients = virtualPatients,
            Stats = stats,
            VirtualPatientCount = virtualPatients.Count,
            AttemptCount = stats.TotalAttempts,
            AvgScore = stats.AvgScore,
        };
    }
}
