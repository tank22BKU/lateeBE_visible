namespace VirtualPatientService.Application.Dtos;

public class ExpertPreviewDto
{
    public string ExpertId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? AvatarUrl { get; set; }
}
