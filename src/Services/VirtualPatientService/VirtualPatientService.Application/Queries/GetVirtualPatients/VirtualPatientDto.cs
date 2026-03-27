namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class VirtualPatientDto
{
    public string PatientId { get; set; } = null!;
    public string ClinicalCaseId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Description { get; set; }
    public string? ChiefConcern { get; set; }
    public object? VitalSigns { get; set; }
    public object? Instructions { get; set; }
    public object? CaseRules { get; set; }
    public object? Persona { get; set; }
}