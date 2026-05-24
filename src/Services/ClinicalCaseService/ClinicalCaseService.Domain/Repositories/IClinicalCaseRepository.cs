using ClinicalCaseService.Domain.Entities;

namespace ClinicalCaseService.Domain.Repositories;

public interface IClinicalCaseRepository
{
    Task<List<ClinicalCase>> GetAllAsync();
    Task<ClinicalCase?> GetByIdAsync(string caseId);
    Task<string?> GetExpertNameAsync(string expertId);
    Task<List<ClinicalCaseLab>> GetLabsByCaseIdAsync(string caseId);
    Task<List<ClinicalCaseRadiology>> GetRadiologyByCaseIdAsync(string caseId);
    Task<List<ClinicalCaseVirtualPatient>> GetVirtualPatientsByCaseIdAsync(string caseId);
    Task<ClinicalCaseStats?> GetStatsByCaseIdAsync(string caseId);
    Task AddAsync(ClinicalCase clinicalCase);
    Task UpdateAsync(ClinicalCase clinicalCase);
    Task DeleteAsync(ClinicalCase clinicalCase);
    Task<(List<ClinicalCase> Items, int Total)> GetPagedAsync(
        string? search,
        string? status,
        string? caseType,
        string? eccId,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize
    );
}
