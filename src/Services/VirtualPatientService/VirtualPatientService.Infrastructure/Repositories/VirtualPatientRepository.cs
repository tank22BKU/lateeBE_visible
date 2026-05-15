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

    public async Task<List<ExpertSummary>> GetExpertsByPatientIdAsync(string patientId)
    {
        var rows = await _db.Database
            .SqlQuery<ExpertRow>($"""
                SELECT
                    e.eid AS ExpertId,
                    u.name AS Name,
                    COALESCE(e.title_position, u.role) AS Role,
                    u.avatar_url AS AvatarUrl,
                    e.bio_quote AS BioQuote,
                    e.education_detail AS EducationDetail,
                    e.expertise_skill AS ExpertiseSkill,
                    u.phone AS Phone,
                    u.email AS Email,
                    u.address AS Location
                FROM expert_virtual_patient_management ev
                INNER JOIN expert e ON ev.expert_id = e.eid
                INNER JOIN users u ON e.eid = u.userid
                WHERE ev.virtual_id = {patientId}
                ORDER BY u.name
                """)
            .ToListAsync();

        return rows.Select(r => new ExpertSummary(
            ExpertId: r.ExpertId ?? string.Empty,
            Name: r.Name ?? string.Empty,
            Role: r.Role,
            AvatarUrl: r.AvatarUrl,
            BioQuote: r.BioQuote,
            EducationDetail: r.EducationDetail,
            ExpertiseSkill: r.ExpertiseSkill,
            Phone: r.Phone,
            Email: r.Email,
            Location: r.Location
        )).ToList();
    }

    private sealed class ExpertRow
    {
        public string? ExpertId { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BioQuote { get; set; }
        public string? EducationDetail { get; set; }
        public string? ExpertiseSkill { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
    }
}