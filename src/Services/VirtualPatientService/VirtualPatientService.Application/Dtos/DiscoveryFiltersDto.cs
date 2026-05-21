namespace VirtualPatientService.Application.Dtos;

public class DiscoveryFiltersDto
{
    public List<string> AvailableLevels { get; set; } = new();
    public List<string> AvailableGenders { get; set; } = new();
    public List<string> AvailableSpecialties { get; set; } = new();
    public List<string> AvailableCaseTypes { get; set; } = new();
}
