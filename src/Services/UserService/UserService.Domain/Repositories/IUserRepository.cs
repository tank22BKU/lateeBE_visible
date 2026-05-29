using UserService.Domain.Entities;

namespace UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();

    Task<User?> GetUserByIdAsync(string userId);

    Task<Expert?> GetExpertByIdAsync(string expertId);
    Task<Expert?> CreateExpertAsync(Expert expert);
    Task<Expert?> UpdateExpertAsync(Expert expert);
    Task<bool> DeleteExpertAsync(string expertId);

    Task<UserDashboardStatistics> GetDashboardStatisticsAsync();

    Task<User> CreateUserAsync(User user);

    Task<User?> UpdateUserAsync(User user);

    Task<bool> DeleteUserAsync(string userId);
}
