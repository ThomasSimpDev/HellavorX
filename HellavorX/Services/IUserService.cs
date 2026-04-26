using HellavorX.Models;
using HellavorX.ViewModels;

namespace HellavorX.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<List<ApplicationUser>> SearchUsersAsync(string query);
    Task UpdateUserProfileAsync(string userId, string name, string bio, SelectedFile? profilePicture);
}
