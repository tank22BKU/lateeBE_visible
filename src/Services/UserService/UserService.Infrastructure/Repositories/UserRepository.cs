using System.Globalization;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _db;

    public UserRepository(UserDbContext db)
    {
        _db = db;
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        return _db.Users.AsNoTracking().ToListAsync();
    }

    public Task<User?> GetUserByIdAsync(string userId)
    {
        return _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.UserId = string.IsNullOrWhiteSpace(user.UserId) ? Guid.NewGuid().ToString() : user.UserId.Trim();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        var existing = await _db.Users.FindAsync(user.UserId);
        if (existing is null)
        {
            return null;
        }

        if (!String.IsNullOrWhiteSpace(user.Name))
        {
            existing.Name = user.Name;    
        }
        
        // EMAIL CANNOT BE UPDATED
        //existing.Email = user.Email;
        
        if (!String.IsNullOrWhiteSpace(user.Password))
        {
            existing.Password = user.Password;
        }

        if (!String.IsNullOrWhiteSpace(user.Phone))
        {
            existing.Phone = user.Phone;    
        }

        if (user.Birthday.HasValue)
        {
            existing.Birthday = user.Birthday;    
        }

        if (!String.IsNullOrWhiteSpace(user.Role))
        {
            existing.Role = user.Role;    
        }

        if (!String.IsNullOrWhiteSpace(user.Status))
        {
            existing.Status = user.Status;    
        }

        if (!String.IsNullOrWhiteSpace(user.Gender))
        {
            existing.Gender = user.Gender;   
        }

        if (!String.IsNullOrWhiteSpace(user.Address))
        {
            existing.Address = user.Address;  
        }
        
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var existing = await _db.Users.FindAsync(userId);
        if (existing is null)
        {
            return false;
        }

        _db.Users.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}