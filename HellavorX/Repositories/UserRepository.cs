using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Users.FindAsync(userId);
    }

    public async Task<List<ApplicationUser>> SearchUsersAsync(string query)
    {
        using var context = _contextFactory.CreateDbContext();
        var lowered = query.ToLower();
        return await context.Users
            .Where(u => u.UserName != null && (u.UserName.ToLower().Contains(lowered) || u.Name.ToLower().Contains(lowered)))
            .OrderBy(u => u.Name)
            .Take(20)
            .ToListAsync();
    }

    public async Task UpdateUserAsync(ApplicationUser user)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}

