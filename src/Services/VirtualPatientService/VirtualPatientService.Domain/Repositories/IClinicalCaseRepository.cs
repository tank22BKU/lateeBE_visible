namespace VirtualPatientService.Domain.Repositories;

public interface IClinicalCaseRepository
{
	Task<Entities.ClinicalCase?> GetByIdAsync(string caseId);
	Task<Dictionary<string, Entities.ClinicalCase>> GetByIdsAsync(IEnumerable<string> caseIds);
}