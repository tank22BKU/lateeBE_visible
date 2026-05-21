namespace VirtualPatientService.Application.Dtos;

public class ExpertDto
{
    public string ExpertId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
}
