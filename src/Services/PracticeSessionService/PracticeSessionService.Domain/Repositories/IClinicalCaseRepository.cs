using PracticeSessionService.Domain.Entities;

namespace PracticeSessionService.Domain.Repositories;

public interface IClinicalCaseRepository
{
	Task<(List<ClinicalCase> Items, int Total)> GetPagedAsync(string? status, int page, int pageSize);
}