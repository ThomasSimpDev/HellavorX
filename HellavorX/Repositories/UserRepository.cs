using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task UpdateUserAsync(ApplicationUser user)
    {
        // Note: Don't call Update() here - the entity is already tracked by the context
        // (e.g., when fetched with FindAsync). Calling Update() on an already-tracked
        // entity can cause issues with change tracking in EF Core.
        await _context.SaveChangesAsync();
    }
}
