using HellavorX.Models;

namespace HellavorX.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task UpdateUserAsync(ApplicationUser user);
}
