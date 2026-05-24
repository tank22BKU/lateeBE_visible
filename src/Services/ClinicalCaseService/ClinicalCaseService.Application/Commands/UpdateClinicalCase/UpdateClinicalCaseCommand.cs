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
    string? CreatedBy,
    string? EccId
) : IRequest<bool>;

public class UpdateClinicalCaseHandler : IRequestHandler<UpdateClinicalCaseCommand, bool>
{
    private readonly IClinicalCaseRepository _repo;

    public UpdateClinicalCaseHandler(IClinicalCaseRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(
        UpdateClinicalCaseCommand request,
        CancellationToken cancellationToken
    )
    {
        var clinicalCase = await _repo.GetByIdAsync(request.CaseId);

        if (clinicalCase == null)
        {
            return false;
        }

        var hasChanges = false;

        if (!string.Equals(clinicalCase.Title, request.Title, StringComparison.Ordinal))
        {
            clinicalCase.Title = request.Title;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.Description, request.Description, StringComparison.Ordinal))
        {
            clinicalCase.Description = request.Description;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.CaseType, request.CaseType, StringComparison.Ordinal))
        {
            clinicalCase.CaseType = request.CaseType;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.Status, request.Status, StringComparison.Ordinal))
        {
            clinicalCase.Status = request.Status;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.Pe, request.Pe, StringComparison.Ordinal))
        {
            clinicalCase.Pe = request.Pe;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.Symptom, request.Symptom, StringComparison.Ordinal))
        {
            clinicalCase.Symptom = request.Symptom;
            hasChanges = true;
        }

        if (
            !string.Equals(
                clinicalCase.MedicalHistory,
                request.MedicalHistory,
                StringComparison.Ordinal
            )
        )
        {
            clinicalCase.MedicalHistory = request.MedicalHistory;
            hasChanges = true;
        }

        if (!string.Equals(clinicalCase.EccId, request.EccId, StringComparison.Ordinal))
        {
            clinicalCase.EccId = request.EccId;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return true;
        }

        clinicalCase.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(clinicalCase);
        return true;
    }
}
