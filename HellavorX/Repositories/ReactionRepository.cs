using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class ReactionRepository : IReactionRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ReactionRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task ToggleReactionAsync(int? postId, int? commentId, string userId, ReactionType type)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Database.BeginTransaction();

        try
        {
            // Remove other types from same user
            var otherReactions = await context.Reactions
                .Where(r => r.UserId == userId 
                    && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId))
                    && r.Type != type)
                .ToListAsync();
            context.Reactions.RemoveRange(otherReactions);

            // Remove existing reaction of same type if exists
            var existing = await context.Reactions
                .FirstOrDefaultAsync(r => r.UserId == userId 
                    && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId))
                    && r.Type == type);
            if (existing != null)
            {
                context.Reactions.Remove(existing);
            }
            else
            {
                // Add new reaction
                var reaction = new Reaction
                {
                    PostId = postId,
                    CommentId = commentId,
                    UserId = userId,
                    Type = type
                };
                context.Reactions.Add(reaction);
            }

            await context.SaveChangesAsync();
            context.Database.CommitTransaction();
        }
        catch
        {
            context.Database.RollbackTransaction();
            throw;
        }
    }

    public async Task<Reaction?> GetReactionAsync(int? postId, int? commentId, string userId, ReactionType type)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reactions
            .FirstOrDefaultAsync(r => r.UserId == userId 
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId))
                && r.Type == type);
    }

    public async Task<Reaction> CreateReactionAsync(Reaction reaction)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Reactions.Add(reaction);
        await context.SaveChangesAsync();
        return reaction;
    }

    public async Task DeleteReactionAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var reaction = await context.Reactions.FindAsync(id);
        if (reaction != null)
        {
            context.Reactions.Remove(reaction);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetLikeCountAsync(int? postId, int? commentId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reactions
            .CountAsync(r => r.Type == ReactionType.Like
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId)));
    }

    public async Task<List<Reaction>> GetReactionsForPostAsync(int postId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reactions
            .Where(r => r.PostId == postId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<List<Reaction>> GetReactionsForCommentAsync(int commentId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reactions
            .Where(r => r.CommentId == commentId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<ReactionType?> GetUserReactionAsync(int? postId, int? commentId, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        var reaction = await context.Reactions
            .FirstOrDefaultAsync(r => r.UserId == userId 
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId)));
        return reaction?.Type;
    }

    public async Task DeleteReactionsByUserExceptType(int? postId, int? commentId, string userId, ReactionType keepType)
    {
        using var context = _contextFactory.CreateDbContext();
        var reactionsToDelete = await context.Reactions
            .Where(r => r.UserId == userId 
                && ((postId.HasValue && r.PostId == postId && !r.CommentId.HasValue) || (commentId.HasValue && r.CommentId == commentId && !r.PostId.HasValue))
                && r.Type != keepType)
            .ToListAsync();
        
        context.Reactions.RemoveRange(reactionsToDelete);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetReactionCountAsync(int? postId, int? commentId, ReactionType type)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Reactions
            .CountAsync(r => ((postId.HasValue && r.PostId == postId.Value && !r.CommentId.HasValue) || (commentId.HasValue && r.CommentId == commentId.Value && !r.PostId.HasValue))
                && r.Type == type);
    }
}

