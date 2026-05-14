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
}
