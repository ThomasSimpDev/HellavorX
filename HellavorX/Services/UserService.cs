using HellavorX.Models;
using HellavorX.Repositories;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly SupabaseStorageService _fileUploadService;

    public UserService(IUserRepository userRepository, SupabaseStorageService fileUploadService)
    {
        _userRepository = userRepository;
        _fileUploadService = fileUploadService;
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task UpdateUserProfileAsync(string userId, string name, string bio, IBrowserFile? profilePicture)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return;

        user.Name = name;
        user.Bio = bio;

        if (profilePicture != null)
        {
            using var stream = profilePicture.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            user.ProfilePictureUrl = await _fileUploadService.UploadFileAsync(stream, profilePicture.Name);
        }

        await _userRepository.UpdateUserAsync(user);
    }
}
