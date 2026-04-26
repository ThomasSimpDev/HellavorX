using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public FollowRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.UserFollows.AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
    }

    public async Task FollowUserAsync(string followerId, string followingId)
    {
        using var context = _contextFactory.CreateDbContext();
        if (followerId == followingId)
            return;

        var exists = await context.UserFollows.AnyAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
        if (!exists)
        {
            context.UserFollows.Add(new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }

    public async Task UnfollowUserAsync(string followerId, string followingId)
    {
        using var context = _contextFactory.CreateDbContext();
        var follow = await context.UserFollows.FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == followingId);
        if (follow != null)
        {
            context.UserFollows.Remove(follow);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetFollowerCountAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.UserFollows.CountAsync(uf => uf.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.UserFollows.CountAsync(uf => uf.FollowerId == userId);
    }

    public async Task<List<string>> GetFollowingIdsAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.UserFollows
            .Where(uf => uf.FollowerId == userId)
            .Select(uf => uf.FollowingId)
            .ToListAsync();
    }
}

