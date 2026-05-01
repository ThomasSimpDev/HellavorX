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

    public async Task<List<Post>> GetAllPostsAsync(int skip = 0, int take = 20)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Posts
            .Include(p => p.User)
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
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

    public async Task<List<Post>> GetPostsByUserIdAsync(string userId, int skip = 0, int take = 20)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Posts
            .Include(p => p.MediaFiles)
            .Include(p => p.Comments)
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Post>> GetFeedForUserAsync(string userId, List<string> followingIds, int skip = 0, int take = 20)
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
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Post>> SearchPostsAsync(string query, int skip = 0, int take = 20)
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
            .Skip(skip)
            .Take(take)
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
            // Remove reactions linked directly to the post
            var postReactions = await context.Reactions.Where(r => r.PostId == id).ToListAsync();
            context.Reactions.RemoveRange(postReactions);

            // Remove reactions linked to the post's comments (comments cascade, but their reactions do not)
            var commentIds = await context.Comments.Where(c => c.PostId == id).Select(c => c.Id).ToListAsync();
            var commentReactions = await context.Reactions.Where(r => commentIds.Contains(r.CommentId!.Value)).ToListAsync();
            context.Reactions.RemoveRange(commentReactions);

            // Remove media linked directly to the post
            var postMedia = await context.MediaFiles.Where(m => m.PostId == id).ToListAsync();
            context.MediaFiles.RemoveRange(postMedia);

            // Remove media linked to the post's comments
            var commentMedia = await context.MediaFiles.Where(m => commentIds.Contains(m.CommentId!.Value)).ToListAsync();
            context.MediaFiles.RemoveRange(commentMedia);

            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }
    }
}

