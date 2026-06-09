namespace VirtualPatientService.Domain.Entities;

public class Expert
{
    public string ExpertId { get; set; } = null!;
    public string? Ssn { get; set; }
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? TitlePosition { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? SocialLink { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}