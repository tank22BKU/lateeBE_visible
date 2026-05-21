using VirtualPatientService.Domain.Entities;

namespace VirtualPatientService.Domain.Repositories;

public interface IClinicalCaseRepository
{
    Task<ClinicalCase?> GetByIdAsync(string caseId, CancellationToken cancellationToken = default);

    Task<Dictionary<string, ClinicalCase>> GetByIdsAsync(
        IEnumerable<string> caseIds,
        CancellationToken cancellationToken = default);
}