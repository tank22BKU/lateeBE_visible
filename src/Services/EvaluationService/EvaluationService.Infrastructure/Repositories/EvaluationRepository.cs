using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;
using EvaluationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EvaluationService.Infrastructure.Repositories;

public class EvaluationRepository : IEvaluationRepository
{
    private readonly EvaluationDbContext _db;

    public EvaluationRepository(EvaluationDbContext db) => _db = db;

    public async Task<Evaluation?> GetByIdAsync(string id) =>
        await _db.Evaluations.Include(e => e.EpaScores).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId) =>
        await _db
            .Evaluations.Join(
                _db.PracticeSessions,
                eval => eval.PracticeSessionId,
                session => session.Id,
                (eval, session) => new { eval, session }
            )
            .Where(x => x.session.LearnerId == learnerId)
            .OrderByDescending(x => x.eval.CreatedAt)
            .Select(x => x.eval)
            .AsNoTracking()
            .ToListAsync();

    public async Task<PracticeSession?> GetPracticeSessionByIdAsync(string id) =>
        await _db.PracticeSessions.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<List<Warning>> GetWarningsByPracticeSessionIdAsync(
        string practiceSessionId
    ) =>
        await _db
            .Warnings.AsNoTracking()
            .Where(x => x.PracticeSessionId == practiceSessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<ClinicalCaseDiagnosisDto?> GetClinicalDiagnosisByPatientIdAsync(
        string patientId
    )
    {
        var result = await _db
            .Database.SqlQuery<ClinicalCaseDiagnosisRaw>(
                $"""
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
                """
            )
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        return new ClinicalCaseDiagnosisDto(
            CaseId: result.CaseId ?? string.Empty,
            EccId: result.EccId ?? string.Empty,
            CanonicalDiagnosis: result.CanonicalDiagnosis ?? string.Empty,
            DescriptionText: result.DescriptionText ?? string.Empty,
            Symptom: result.Symptom ?? string.Empty,
            MedicalHistory: result.MedicalHistory ?? string.Empty
        );
    }

    public async Task<VirtualPatientRef?> GetVirtualPatientByIdAsync(string patientId)
    {
        var result = await _db
            .Database.SqlQuery<VirtualPatientRaw>(
                $"""
                SELECT
                    patient_id    AS PatientId,
                    time_setting  AS TimeSettingMinutes,
                    argument_time AS ArgumentTimeMinutes
                FROM virtual_patient
                WHERE patient_id = {patientId}
                LIMIT 1
                """
            )
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        return new VirtualPatientRef(
            PatientId: result.PatientId ?? string.Empty,
            TimeSettingMinutes: result.TimeSettingMinutes ?? 30,
            ArgumentTimeMinutes: result.ArgumentTimeMinutes ?? 15
        );
    }

    public async Task<RubricDto?> GetRubricByEccIdAsync(string eccId)
    {
        if (string.IsNullOrWhiteSpace(eccId))
            return null;

        var result = await _db
            .Database.SqlQuery<RubricRaw>(
                $"""
                SELECT id, description, version
                FROM evaluation_clinical_criteria
                WHERE id = {eccId}
                LIMIT 1
                """
            )
            .FirstOrDefaultAsync();

        if (result == null)
            return null;

        return new RubricDto(
            Id: result.Id ?? string.Empty,
            Description: result.Description ?? string.Empty,
            Version: result.Version ?? "1.0.0"
        );
    }

    public async Task<PracticeFeedback?> GetPracticeFeedbackBySessionIdAsync(
        string practiceSessionId
    ) =>
        await _db
            .PracticeFeedbacks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PracticeSessionId == practiceSessionId);

    public async Task<List<EvaluationEpaScore>> GetEpaScoresByEvaluationIdAsync(
        string evaluationId
    ) =>
        await _db
            .EpaScores.AsNoTracking()
            .Where(x => x.EvaluationId == evaluationId)
            .OrderBy(x => x.EpaId)
            .ToListAsync();

    public async Task<List<IssueListItem>> GetIssuesAsync(
        string practiceSessionId,
        string learnerId
    )
    {
        var rows = await _db
            .Database.SqlQuery<IssueRow>(
                $"""
                SELECT
                    i.id AS IssueId,
                    i.learner_id AS LearnerId,
                    u.name AS LearnerName,
                    i.created_at AS CreatedAt,
                    i.label AS Label,
                    i.description AS Description,
                    i.status AS Status,
                    ri.expert_id AS ExpertId,
                    eu.name AS ExpertName,
                    ri.feedback AS Feedback
                FROM issue i
                INNER JOIN users u ON u.userid = i.learner_id
                LEFT JOIN resolved_issue ri ON ri.issue_id = i.id
                LEFT JOIN users eu ON eu.userid = ri.expert_id
                WHERE i.is_deleted = false
                    AND i.practice_session_id = {practiceSessionId}
                    AND i.learner_id = {learnerId}
                ORDER BY i.created_at DESC
                """
            )
            .ToListAsync();

        return rows.GroupBy(r => new
            {
                r.IssueId,
                r.LearnerId,
                r.LearnerName,
                r.CreatedAt,
                r.Label,
                r.Description,
                r.Status,
            })
            .Select(g =>
            {
                var firstExpert = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ExpertId));
                IssueExpertFeedback? expertFeedback = null;
                if (firstExpert != null)
                {
                    expertFeedback = new IssueExpertFeedback(
                        ExpertId: firstExpert.ExpertId ?? string.Empty,
                        ExpertName: firstExpert.ExpertName ?? string.Empty,
                        Feedback: firstExpert.Feedback ?? string.Empty
                    );
                }

                return new IssueListItem(
                    IssueId: g.Key.IssueId ?? string.Empty,
                    LearnerId: g.Key.LearnerId ?? string.Empty,
                    LearnerName: g.Key.LearnerName ?? string.Empty,
                    CreatedAt: g.Key.CreatedAt ?? DateTime.UtcNow,
                    Label: g.Key.Label,
                    Description: g.Key.Description ?? string.Empty,
                    Status: g.Key.Status ?? "Open",
                    ExpertFeedback: expertFeedback
                );
            })
            .ToList();
    }

    public async Task AddEvaluationAsync(Evaluation evaluation) =>
        await _db.Evaluations.AddAsync(evaluation);

    public async Task AddEpaScoresAsync(IEnumerable<EvaluationEpaScore> scores) =>
        await _db.EpaScores.AddRangeAsync(scores);

    public async Task AddWarningsAsync(IEnumerable<Warning> warnings)
    {
        var incomingIds = warnings.Select(w => w.Id).ToList();
        var existingIds = await _db
            .Warnings.Where(w => incomingIds.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingIds);
        var newWarnings = warnings.Where(w => !existingSet.Contains(w.Id)).ToList();

        if (newWarnings.Count > 0)
            await _db.Warnings.AddRangeAsync(newWarnings);
    }

    public async Task AddPracticeFeedbackAsync(PracticeFeedback feedback) =>
        await _db.PracticeFeedbacks.AddAsync(feedback);

    public async Task AddIssueAsync(Issue issue) => await _db.Issues.AddAsync(issue);

    public Task UpdatePracticeSessionAsync(PracticeSession session)
    {
        _db.PracticeSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _db.Evaluations.FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
            _db.Evaluations.Remove(entity);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    private sealed class ClinicalCaseDiagnosisRaw
    {
        public string? CaseId { get; set; }
        public string? EccId { get; set; }
        public string? CanonicalDiagnosis { get; set; }
        public string? DescriptionText { get; set; }
        public string? Symptom { get; set; }
        public string? MedicalHistory { get; set; }
    }

    private sealed class VirtualPatientRaw
    {
        public string? PatientId { get; set; }
        public int? TimeSettingMinutes { get; set; }
        public int? ArgumentTimeMinutes { get; set; }
    }

    private sealed class RubricRaw
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
    }

    private sealed class IssueRow
    {
        public string? IssueId { get; set; }
        public string? LearnerId { get; set; }
        public string? LearnerName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Label { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? ExpertId { get; set; }
        public string? ExpertName { get; set; }
        public string? Feedback { get; set; }
    }
}
