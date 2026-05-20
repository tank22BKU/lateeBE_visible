using System;

namespace UserService.Domain.Entities;

public class Expert
{
    public string ExpertId { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public string? BioQuote { get; set; }
    public string? EducationDetail { get; set; }
    public string? TitlePosition { get; set; }
    public string? ExpertiseSkill { get; set; }
    public string? SocialLink { get; set; }
    public User? User { get; set; }
}