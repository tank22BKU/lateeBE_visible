namespace EvaluationService.Application.Dtos;

public record ClinicalCaseDiagnosisDto(
    string CaseId,
    string CanonicalDiagnosis,   // clinical_case.type
    string DescriptionText,       // clinical_case.description
    string Symptom,
    string MedicalHistory
);