using EvaluationService.Domain.Entities;

namespace EvaluationService.Domain.Repositories;

public interface IEvaluationRepository
{
    Task<Evaluation?>      GetByIdAsync(string id);
    Task<List<Evaluation>> GetByLearnerIdAsync(string learnerId);

    Task<PracticeSession?>          GetPracticeSessionByIdAsync(string id);
    Task<List<Warning>>             GetWarningsByPracticeSessionIdAsync(string practiceSessionId);
    Task<ClinicalCaseDiagnosisDto?> GetClinicalDiagnosisByPatientIdAsync(string patientId);
    Task<VirtualPatientRef?>        GetVirtualPatientByIdAsync(string patientId);
    Task<RubricDto?>                GetRubricByEccIdAsync(string eccId);
    Task<PracticeFeedback?>         GetPracticeFeedbackBySessionIdAsync(string practiceSessionId);

    Task<List<EvaluationEpaScore>>  GetEpaScoresByEvaluationIdAsync(string evaluationId);

    Task AddEvaluationAsync(Evaluation evaluation);
    Task AddEpaScoresAsync(IEnumerable<EvaluationEpaScore> scores);
    Task AddWarningsAsync(IEnumerable<Warning> warnings);
    Task AddPracticeFeedbackAsync(PracticeFeedback feedback);
    Task UpdatePracticeSessionAsync(PracticeSession session);
    Task DeleteAsync(string id);
    Task SaveChangesAsync();
}

public record ClinicalCaseDiagnosisDto(
    string CaseId,
    string EccId,
    string CanonicalDiagnosis,
    string DescriptionText,
    string Symptom,
    string MedicalHistory
);

public record RubricDto(
    string Id,
    string Description,
    string Version
);