using HellavorX.Models;
using Microsoft.AspNetCore.Identity;

namespace HellavorX.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Success, string? Error)> SignupAsync(string name, string username, string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            Name = name
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
            return (true, null);

        return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
        return result.Succeeded;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
