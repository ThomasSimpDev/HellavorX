namespace HellavorX.Services;

public interface IAuthService
{
    Task<(bool Success, string? Error)> SignupAsync(string name, string username, string email, string password);
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
}
