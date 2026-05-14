namespace EvaluationService.Domain.Entities;

public sealed record VirtualPatientRef(
    string PatientId,
    int TimeSettingMinutes,   
    int ArgumentTimeMinutes   
);