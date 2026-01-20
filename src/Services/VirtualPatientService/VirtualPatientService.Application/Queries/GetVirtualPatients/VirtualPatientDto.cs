namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class VirtualPatientDto
{
    public string Id { get; set; } = null!;
    public string? Description { get; set; }
    public string? Behaviors { get; set; }
}
