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
        return _db.Users.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync();
    }

    public Task<User?> GetUserByIdAsync(string userId)
    {
        return _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
    }

    public Task<Expert?> GetExpertByIdAsync(string expertId)
    {
        return _db.Experts.AsNoTracking().FirstOrDefaultAsync(x => x.ExpertId == expertId);
    }

    public async Task<UserDashboardStatistics> GetDashboardStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var currentWindowStart = now.AddDays(-7);
        var previousWindowStart = now.AddDays(-14);

        var users = _db.Users.AsNoTracking().Where(x => !x.IsDeleted);

        var totalLearners = await users.CountAsync(x => x.Role != null && x.Role.ToLower() == "learner");
        var totalExperts = await users.CountAsync(x => x.Role != null && x.Role.ToLower() == "expert");
        var totalAdmins = await users.CountAsync(x => x.Role != null && x.Role.ToLower() == "admin");
        var totalActiveUsers = await users.CountAsync(x => x.Status != null && x.Status.ToLower() == "active");

        var currentUsers = await users.CountAsync(x => x.CreatedAt >= currentWindowStart && x.CreatedAt < now);
        var previousUsers = await users.CountAsync(x => x.CreatedAt >= previousWindowStart && x.CreatedAt < currentWindowStart);

        var currentLearners = await users.CountAsync(x =>
            x.CreatedAt >= currentWindowStart &&
            x.CreatedAt < now &&
            x.Role != null &&
            x.Role.ToLower() == "learner");

        var previousLearners = await users.CountAsync(x =>
            x.CreatedAt >= previousWindowStart &&
            x.CreatedAt < currentWindowStart &&
            x.Role != null &&
            x.Role.ToLower() == "learner");

        return new UserDashboardStatistics
        {
            IncreaseUser = currentUsers - previousUsers,
            TotalLearners = totalLearners,
            IncreaseLearners = currentLearners - previousLearners,
            TotalExperts = totalExperts,
            TotalAdmins = totalAdmins,
            TotalActiveUsers = totalActiveUsers
        };
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.UserId = string.IsNullOrWhiteSpace(user.UserId) ? Guid.NewGuid().ToString() : user.UserId.Trim();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.IsDeleted = false;

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

        if (!String.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            existing.AvatarUrl = user.AvatarUrl;
        }

        existing.UpdatedAt = DateTime.UtcNow;
        
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

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}