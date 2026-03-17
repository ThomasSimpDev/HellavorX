using HellavorX.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public interface IUserService
{
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task UpdateUserProfileAsync(string userId, string name, string bio, IBrowserFile? profilePicture);
}
