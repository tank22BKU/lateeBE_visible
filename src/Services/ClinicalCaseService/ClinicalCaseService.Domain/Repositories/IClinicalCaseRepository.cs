using ClinicalCaseService.Domain.Entities;

namespace ClinicalCaseService.Domain.Repositories;

public interface IClinicalCaseRepository
{
    Task<ClinicalCase?> GetByIdAsync(string id);
    Task<List<ClinicalCase>> GetActiveAsync(int limit);

    Task<(List<ClinicalCase> Items, int Total)>
        GetPagedAsync(string? status, int page, int pageSize);
}
