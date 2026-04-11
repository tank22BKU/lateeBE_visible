using VirtualPatientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.Infrastructure.Repositories;

public class VirtualPatientRepository : IVirtualPatientRepository
{
    private readonly VirtualPatientDbContext _db;

    public VirtualPatientRepository(VirtualPatientDbContext db)
    {
        _db = db;
    }

    public async Task<List<VirtualPatient>> GetAllAsync()
    {
        return await _db.VirtualPatients
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<VirtualPatient?> GetByIdAsync(string patientId)
    {
        return _db.VirtualPatients
            .FirstOrDefaultAsync(x => x.PatientId == patientId);
    }

    public async Task<(List<VirtualPatient> Items, int Total)> GetPagedAsync(string? gender, int page, int pageSize)
    {
        var query = _db.VirtualPatients.AsNoTracking();

        if (!string.IsNullOrEmpty(gender))
        {
            query = query.Where(x => x.Gender == gender);
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