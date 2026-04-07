using HellavorX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HellavorX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
    {
        // Find user by username or email
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(username);
        }
        
        if (user == null)
        {
            return Redirect("/login?error=User+not+found");
        }
        
        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
        
        if (result.Succeeded)
        {
            return Redirect("/");
        }
        
        return Redirect("/login?error=Invalid+credentials");
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromForm] string name, [FromForm] string username, [FromForm] string email, [FromForm] string password)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            Name = name
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Redirect("/profile");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Redirect($"/signup?error={Uri.EscapeDataString(errors)}");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/login");
    }
}
