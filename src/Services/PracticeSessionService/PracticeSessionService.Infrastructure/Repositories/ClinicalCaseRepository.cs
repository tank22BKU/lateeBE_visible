using Microsoft.EntityFrameworkCore;
using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;
using PracticeSessionService.Infrastructure.Persistance;

namespace PracticeSessionService.Infrastructure.Repositories;

public class ClinicalCaseRepository : IClinicalCaseRepository
{
	private readonly PracticeSessionDbContext _db;

	public ClinicalCaseRepository(PracticeSessionDbContext db)
	{
		_db = db;
	}

	public async Task<(List<ClinicalCase> Items, int Total)> GetPagedAsync(string? status, int page, int pageSize)
	{
		var query = _db.ClinicalCases.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(status))
		{
			query = query.Where(x => x.Status == status);
		}

		var total = await query.CountAsync();

		var items = await query
			.OrderByDescending(x => x.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return (items, total);
	}
}