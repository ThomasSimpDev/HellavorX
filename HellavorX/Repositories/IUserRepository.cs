using HellavorX.Models;

namespace HellavorX.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<List<ApplicationUser>> SearchUsersAsync(string query);
    Task UpdateUserAsync(ApplicationUser user);
}
