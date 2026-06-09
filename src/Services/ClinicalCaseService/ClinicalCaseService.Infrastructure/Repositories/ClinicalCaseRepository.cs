using ClinicalCaseService.Domain.Entities;
using ClinicalCaseService.Domain.Repositories;
using ClinicalCaseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicalCaseService.Infrastructure.Repositories;

public class ClinicalCaseRepository : IClinicalCaseRepository
{
    private readonly ClinicalCaseDbContext _db;

    public ClinicalCaseRepository(ClinicalCaseDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClinicalCase>> GetAllAsync()
    {
        return await _db
            .ClinicalCases.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public Task<ClinicalCase?> GetByIdAsync(string caseId)
    {
        return _db.ClinicalCases.AsNoTracking().FirstOrDefaultAsync(x => x.CaseId == caseId);
    }

    public async Task<string?> GetExpertNameAsync(string expertId)
    {
        var row = await _db
            .Database.SqlQuery<ExpertNameRow>(
                $"""
                SELECT name AS Name
                FROM users
                WHERE userid = {expertId}
                LIMIT 1
                """
            )
            .FirstOrDefaultAsync();

        return row?.Name;
    }

    public async Task<bool> ExpertExistsAsync(string expertId)
    {
        var row = await _db
            .Database.SqlQuery<ExistsRow>(
                $"""
                SELECT COUNT(1) AS Value
                FROM expert
                WHERE eid = {expertId}
                """
            )
            .FirstAsync();

        return row.Value > 0;
    }

    public async Task<bool> EvaluationCriteriaExistsAsync(string eccId)
    {
        var row = await _db
            .Database.SqlQuery<ExistsRow>(
                $"""
                SELECT COUNT(1) AS Value
                FROM evaluation_clinical_criteria
                WHERE id = {eccId}
                """
            )
            .FirstAsync();

        return row.Value > 0;
    }

    public Task<List<ClinicalCaseLab>> GetLabsByCaseIdAsync(string caseId)
    {
        return _db
            .Database.SqlQuery<ClinicalCaseLab>(
                $"""
                SELECT
                    lt.id AS Id,
                    lti.itemid AS ItemId,
                    lti.label AS Label,
                    lti.fluid AS Fluid,
                    lti.category AS Category,
                    lt.value AS Value,
                    lt.rangelower AS RangeLower,
                    lt.rangeupper AS RangeUpper
                FROM laboratorytest lt
                INNER JOIN labtestitem lti ON lti.itemid = lt.itemid
                WHERE lt.clinicalcase_id = {caseId}
                ORDER BY lt.id ASC
                """
            )
            .ToListAsync();
    }

    public Task<List<ClinicalCaseRadiology>> GetRadiologyByCaseIdAsync(string caseId)
    {
        return _db
            .Database.SqlQuery<ClinicalCaseRadiology>(
                $"""
                SELECT
                    rr.id AS Id,
                    rr.noteid AS NoteId,
                    rr.modality AS Modality,
                    rr.region AS Region,
                    rr.examname AS ExamName,
                    rr.text AS Text
                FROM radiologyreport rr
                WHERE rr.clinicalcase_id = {caseId}
                ORDER BY rr.id ASC
                """
            )
            .ToListAsync();
    }

    public Task<List<ClinicalCaseVirtualPatient>> GetVirtualPatientsByCaseIdAsync(string caseId)
    {
        return _db
            .Database.SqlQuery<ClinicalCaseVirtualPatient>(
                $"""
                SELECT
                    vp.patient_id AS PatientId,
                    vp.name AS Name,
                    vp.age AS Age,
                    vp.gender AS Gender,
                    vp.level AS Level,
                    vp.status AS Status
                FROM virtual_patient vp
                WHERE vp.case_id = {caseId}
                ORDER BY vp.created_at ASC
                """
            )
            .ToListAsync();
    }

    public async Task<ClinicalCaseStats?> GetStatsByCaseIdAsync(string caseId)
    {
        var row = await _db
            .Database.SqlQuery<ClinicalCaseStatsRow>(
                $"""
                SELECT
                    COALESCE(COUNT(DISTINCT vp.patient_id), 0) AS VirtualPatientCount,
                    COALESCE(COUNT(DISTINCT ps.id), 0) AS TotalAttempts,
                    COALESCE(ROUND(AVG(e.score), 2), 0) AS AvgScore,
                    COALESCE(
                        ROUND(
                            SUM(CASE WHEN ps.status = 'Completed' THEN 1 ELSE 0 END) / NULLIF(COUNT(ps.id), 0),
                            2
                        ),
                        0
                    ) AS CompletionRate
                FROM clinical_case cc
                LEFT JOIN virtual_patient vp ON vp.case_id = cc.case_id
                LEFT JOIN practice_sessions ps ON ps.patient_id = vp.patient_id
                LEFT JOIN evaluation e ON e.practice_session_id = ps.id
                WHERE cc.case_id = {caseId}
                """
            )
            .FirstOrDefaultAsync();

        if (row == null)
        {
            return null;
        }

        return new ClinicalCaseStats
        {
            VirtualPatientCount = row.VirtualPatientCount,
            TotalAttempts = row.TotalAttempts,
            AvgScore = row.AvgScore,
            CompletionRate = row.CompletionRate,
        };
    }

    public async Task AddAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Add(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Update(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ClinicalCase clinicalCase)
    {
        _db.ClinicalCases.Remove(clinicalCase);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<ClinicalCase> Items, int Total)> GetPagedAsync(
        string? search,
        string? status,
        string? caseType,
        string? eccId,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize
    )
    {
        var clinicalCasesQuery = _db.ClinicalCases.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            clinicalCasesQuery = clinicalCasesQuery.Where(x =>
                x.Title.Contains(search) || x.CaseId.Contains(search)
            );
        }

        if (!string.IsNullOrEmpty(status))
        {
            clinicalCasesQuery = clinicalCasesQuery.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(caseType))
        {
            clinicalCasesQuery = clinicalCasesQuery.Where(x => x.CaseType == caseType);
        }

        if (!string.IsNullOrWhiteSpace(eccId))
        {
            clinicalCasesQuery = clinicalCasesQuery.Where(x => x.EccId == eccId);
        }

        var isDescending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);

        clinicalCasesQuery = (sortBy?.ToLowerInvariant(), isDescending) switch
        {
            ("updatedat", false) => clinicalCasesQuery.OrderBy(x => x.UpdatedAt),
            ("updatedat", true) => clinicalCasesQuery.OrderByDescending(x => x.UpdatedAt),
            ("title", false) => clinicalCasesQuery.OrderBy(x => x.Title),
            ("title", true) => clinicalCasesQuery.OrderByDescending(x => x.Title),
            ("createdat", false) => clinicalCasesQuery.OrderBy(x => x.CreatedAt),
            ("createdat", true) => clinicalCasesQuery.OrderByDescending(x => x.CreatedAt),
            _ => clinicalCasesQuery.OrderByDescending(x => x.CreatedAt),
        };

        var total = await clinicalCasesQuery.CountAsync();

        var items = await clinicalCasesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    private sealed class ExpertNameRow
    {
        public string? Name { get; set; }
    }

    private sealed class ExistsRow
    {
        public int Value { get; set; }
    }

    private sealed class ClinicalCaseStatsRow
    {
        public int VirtualPatientCount { get; set; }
        public int TotalAttempts { get; set; }
        public decimal AvgScore { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
