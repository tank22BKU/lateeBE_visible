using ClinicalCaseService.Application.Queries.GetClinicalCases;
using ClinicalCaseService.Domain.Entities;
using ClinicalCaseService.Domain.Repositories;
using MediatR;

namespace ClinicalCaseService.Application.Commands.CreateClinicalCase;

public record CreateClinicalCaseCommand(
    string CaseId,
    string Title,
    string? Description,
    string? CaseType,
    string? Status,
    string? Pe,
    string? Symptom,
    string? MedicalHistory,
    string? CreatedBy,
    string EccId = "CRIT-001"
) : IRequest<ClinicalCaseDto>;

public class CreateClinicalCaseHandler : IRequestHandler<CreateClinicalCaseCommand, ClinicalCaseDto>
{
    private readonly IClinicalCaseRepository _repo;

    public CreateClinicalCaseHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<ClinicalCaseDto> Handle(
        CreateClinicalCaseCommand request,
        CancellationToken cancellationToken
    )
    {
        var createdBy =
            request.CreatedBy
            ?? throw new InvalidOperationException(
                "CreatedBy must be resolved before creating a clinical case."
            );

        if (!await _repo.ExpertExistsAsync(createdBy))
        {
            throw new ArgumentException(
                $"Expert '{createdBy}' does not exist, so clinical case cannot be created."
            );
        }

        if (!await _repo.EvaluationCriteriaExistsAsync(request.EccId))
        {
            throw new ArgumentException(
                $"Evaluation criteria '{request.EccId}' does not exist, so clinical case cannot be created."
            );
        }

        var clinicalCase = new ClinicalCase
        {
            CaseId = request.CaseId,
            Title = request.Title,
            Description = request.Description,
            CaseType = request.CaseType,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status,
            Pe = request.Pe,
            Symptom = request.Symptom,
            MedicalHistory = request.MedicalHistory,
            CreatedBy = createdBy,
            EccId = request.EccId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddAsync(clinicalCase);

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
            UpdatedAt = clinicalCase.UpdatedAt,
        };
    }
}
