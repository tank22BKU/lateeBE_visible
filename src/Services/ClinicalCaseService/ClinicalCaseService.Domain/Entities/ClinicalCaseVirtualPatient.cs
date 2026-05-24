namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCaseVirtualPatient
{
    public string? PatientId { get; set; }
    public string? Name { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
}
