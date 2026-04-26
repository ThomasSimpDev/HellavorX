using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public PostRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Posts
            .Include(p => p.User)
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Posts
            .Include(p => p.User)
            .Include(p => p.MediaFiles)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Posts
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Post>> GetFeedForUserAsync(string userId, List<string> followingIds)
    {
        using var context = _contextFactory.CreateDbContext();
        var feedUserIds = followingIds.ToList();
        feedUserIds.Add(userId);

        return await context.Posts
            .Include(p => p.User)
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .Where(p => feedUserIds.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Post>> SearchPostsAsync(string query)
    {
        using var context = _contextFactory.CreateDbContext();
        var lowered = query.ToLower();
        return await context.Posts
            .Include(p => p.User)
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .Where(p => p.Content.ToLower().Contains(lowered))
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }

    public async Task UpdatePostAsync(Post post)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Posts.Update(post);
        await context.SaveChangesAsync();
    }

    public async Task DeletePostAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var post = await context.Posts.FindAsync(id);
        if (post != null)
        {
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }
    }
}

