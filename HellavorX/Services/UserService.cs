using HellavorX.Models;
using HellavorX.Repositories;
using HellavorX.ViewModels;
using Microsoft.Extensions.Logging;

namespace HellavorX.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly SupabaseStorageService _fileUploadService;
    private readonly ILogger<UserService>? _logger;

    public UserService(IUserRepository userRepository, SupabaseStorageService fileUploadService, ILogger<UserService>? logger = null)
    {
        _userRepository = userRepository;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task UpdateUserProfileAsync(string userId, string name, string bio, SelectedFile? profilePicture)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger?.LogWarning("User not found with ID: {UserId}", userId);
                return;
            }

            _logger?.LogInformation("Updating profile for user {UserId}. ProfilePicture provided: {HasPicture}", 
                userId, profilePicture != null);

            user.Name = name;
            user.Bio = bio;

            if (profilePicture != null)
            {
                _logger?.LogInformation("Uploading profile picture: {FileName}, Size: {Size} bytes", 
                    profilePicture.Name, profilePicture.Content.Length);
                    
                using var stream = new MemoryStream(profilePicture.Content);
                user.ProfilePictureUrl = await _fileUploadService.UploadFileAsync(stream, profilePicture.Name);
                
                _logger?.LogInformation("Profile picture uploaded successfully. New URL: {Url}", user.ProfilePictureUrl);
            }

            await _userRepository.UpdateUserAsync(user);
            
            _logger?.LogInformation("User profile updated successfully in database for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating profile for user {UserId}", userId);
            throw;
        }
    }
}
