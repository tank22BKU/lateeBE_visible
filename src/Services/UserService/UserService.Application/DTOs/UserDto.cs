namespace UserService.Application.DTOs;

public class UserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Profile? Profile { get; set; }
}

public class Profile
{
    public string Id { get; set; } 
    public string Ssn { get; set; }
    public string? BioQoute { get; set; } = String.Empty;
    public string? EducationDetail { get; set; } = String.Empty;
    public string? TitlePosition { get; set; } = String.Empty;
    public string? ExpertiseSkill {get; set; } = String.Empty;
    public string? SocialLink { get; set; } = String.Empty;
}

