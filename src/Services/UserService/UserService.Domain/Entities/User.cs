using System;

namespace UserService.Domain.Entities;

public class User
{
    public string UserId { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Password { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
