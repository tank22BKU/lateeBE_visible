using ClinicalCaseService.Domain.Entities;

namespace ClinicalCaseService.Domain.Repositories;

public interface IClinicalCaseRepository
{
    Task<List<ClinicalCase>> GetAllAsync();
    Task<ClinicalCase?> GetByIdAsync(string caseId);
    Task AddAsync(ClinicalCase clinicalCase);
    Task UpdateAsync(ClinicalCase clinicalCase);
    Task DeleteAsync(ClinicalCase clinicalCase);
    Task<(List<ClinicalCase> Items, int Total)>
        GetPagedAsync(string? status, int page, int pageSize);
}
