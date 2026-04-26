using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CommentRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.User)
            .Include(c => c.MediaFiles)
            .Include(c => c.Reactions)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Comments
            .Include(c => c.User)
            .Include(c => c.MediaFiles)
            .Include(c => c.Reactions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateCommentAsync(Comment comment)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Comments.Update(comment);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var comment = await context.Comments.FindAsync(id);
        if (comment != null)
        {
            var reactions = await context.Reactions.Where(r => r.CommentId == id).ToListAsync();
            context.Reactions.RemoveRange(reactions);

            var media = await context.MediaFiles.Where(m => m.CommentId == id).ToListAsync();
            context.MediaFiles.RemoveRange(media);

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteCommentWithRepliesAsync(int commentId, int postId)
    {
        using var context = _contextFactory.CreateDbContext();

        // Delete all replies recursively.
        // Use projection so reply entities are not tracked in this context;
        // otherwise EF tries to fix-up their ParentCommentId when we remove
        // the parent, but the recursive calls delete them in *other* contexts.
        var replyIds = await context.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentId == commentId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var replyId in replyIds)
        {
            await DeleteCommentWithRepliesAsync(replyId, postId);
        }

        var comment = await context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            var reactions = await context.Reactions.Where(r => r.CommentId == commentId).ToListAsync();
            context.Reactions.RemoveRange(reactions);

            var media = await context.MediaFiles.Where(m => m.CommentId == commentId).ToListAsync();
            context.MediaFiles.RemoveRange(media);

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();
        }
    }
}
