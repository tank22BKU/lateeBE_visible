using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EvaluationService.Infrastructure.Repositories;

public class EvaluationRepository : IEvaluationRepository
{
    private readonly EvaluationDbContext _db;

    public EvaluationRepository(EvaluationDbContext db) => _db = db;

    public async Task<Evaluation?> GetByIdAsync(string id)
        => await _db.Evaluations
            .Include(e => e.EpaScores)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId)
        => await _db.Evaluations
            .Join(_db.PracticeSessions,
                eval    => eval.PracticeSessionId,
                session => session.Id,
                (eval, session) => new { eval, session })
            .Where(x => x.session.LearnerId == learnerId)
            .OrderByDescending(x => x.eval.CreatedAt)
            .Select(x => x.eval)
            .AsNoTracking()
            .ToListAsync();

    public async Task<PracticeSession?> GetPracticeSessionByIdAsync(string id)
        => await _db.PracticeSessions.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(string practiceSessionId)
        => await _db.Warnings
            .AsNoTracking()
            .Where(x => x.PracticeSessionId == practiceSessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<ClinicalCaseDiagnosisDto?> GetClinicalDiagnosisByPatientIdAsync(string patientId)
    {
        var result = await _db.Database
            .SqlQuery<ClinicalCaseDiagnosisRaw>($"""
                SELECT
                    cc.case_id        AS CaseId,
                    cc.eccid          AS EccId,
                    cc.type           AS CanonicalDiagnosis,
                    cc.description    AS DescriptionText,
                    cc.symptom        AS Symptom,
                    cc.medicalhistory AS MedicalHistory
                FROM virtual_patient vp
                INNER JOIN clinical_case cc ON vp.case_id = cc.case_id
                WHERE vp.patient_id = {patientId}
                LIMIT 1
                """)
            .FirstOrDefaultAsync();

        if (result == null) return null;

        return new ClinicalCaseDiagnosisDto(
            CaseId:             result.CaseId             ?? string.Empty,
            EccId:              result.EccId              ?? string.Empty,
            CanonicalDiagnosis: result.CanonicalDiagnosis ?? string.Empty,
            DescriptionText:    result.DescriptionText    ?? string.Empty,
            Symptom:            result.Symptom            ?? string.Empty,
            MedicalHistory:     result.MedicalHistory     ?? string.Empty
        );
    }

    public async Task<VirtualPatientRef?> GetVirtualPatientByIdAsync(string patientId)
    {
        var result = await _db.Database
            .SqlQuery<VirtualPatientRaw>($"""
                SELECT
                    patient_id    AS PatientId,
                    time_setting  AS TimeSettingMinutes,
                    argument_time AS ArgumentTimeMinutes
                FROM virtual_patient
                WHERE patient_id = {patientId}
                LIMIT 1
                """)
            .FirstOrDefaultAsync();

        if (result == null) return null;

        return new VirtualPatientRef(
            PatientId:           result.PatientId          ?? string.Empty,
            TimeSettingMinutes:  result.TimeSettingMinutes  ?? 30,
            ArgumentTimeMinutes: result.ArgumentTimeMinutes ?? 15
        );
    }

    public async Task<RubricDto?> GetRubricByEccIdAsync(string eccId)
    {
        if (string.IsNullOrWhiteSpace(eccId)) return null;

        var result = await _db.Database
            .SqlQuery<RubricRaw>($"""
                SELECT id, description, version
                FROM evaluation_clinical_criteria
                WHERE id = {eccId}
                LIMIT 1
                """)
            .FirstOrDefaultAsync();

        if (result == null) return null;

        return new RubricDto(
            Id:          result.Id          ?? string.Empty,
            Description: result.Description ?? string.Empty,
            Version:     result.Version     ?? "1.0.0"
        );
    }

    public async Task<PracticeFeedback?> GetPracticeFeedbackBySessionIdAsync(string practiceSessionId)
        => await _db.PracticeFeedbacks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PracticeSessionId == practiceSessionId);


    public async Task<List<EvaluationEpaScore>> GetEpaScoresByEvaluationIdAsync(string evaluationId)
        => await _db.EpaScores
            .AsNoTracking()
            .Where(x => x.EvaluationId == evaluationId)
            .OrderBy(x => x.EpaId)
            .ToListAsync();

    public async Task AddEvaluationAsync(Evaluation evaluation)
        => await _db.Evaluations.AddAsync(evaluation);

    public async Task AddEpaScoresAsync(IEnumerable<EvaluationEpaScore> scores)
        => await _db.EpaScores.AddRangeAsync(scores);

    public async Task AddWarningsAsync(IEnumerable<Warning> warnings)
    {
        var incomingIds = warnings.Select(w => w.Id).ToList();
        var existingIds = await _db.Warnings
            .Where(w => incomingIds.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingIds);
        var newWarnings = warnings.Where(w => !existingSet.Contains(w.Id)).ToList();

        if (newWarnings.Count > 0)
            await _db.Warnings.AddRangeAsync(newWarnings);
    }

    public async Task AddPracticeFeedbackAsync(PracticeFeedback feedback)
        => await _db.PracticeFeedbacks.AddAsync(feedback);

    public Task UpdatePracticeSessionAsync(PracticeSession session)
    {
        _db.PracticeSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _db.Evaluations.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null) _db.Evaluations.Remove(entity);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();


    private sealed class ClinicalCaseDiagnosisRaw
    {
        public string? CaseId             { get; set; }
        public string? EccId              { get; set; }
        public string? CanonicalDiagnosis { get; set; }
        public string? DescriptionText    { get; set; }
        public string? Symptom            { get; set; }
        public string? MedicalHistory     { get; set; }
    }

    private sealed class VirtualPatientRaw
    {
        public string? PatientId            { get; set; }
        public int?    TimeSettingMinutes   { get; set; }
        public int?    ArgumentTimeMinutes  { get; set; }
    }

    private sealed class RubricRaw
    {
        public string? Id          { get; set; }
        public string? Description { get; set; }
        public string? Version     { get; set; }
    }
}