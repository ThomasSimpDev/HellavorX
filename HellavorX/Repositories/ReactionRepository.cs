using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class ReactionRepository : IReactionRepository
{
    private readonly ApplicationDbContext _context;

    public ReactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reaction?> GetReactionAsync(int? postId, int? commentId, string userId, ReactionType type)
    {
        return await _context.Reactions
            .FirstOrDefaultAsync(r => r.UserId == userId 
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId))
                && r.Type == type);
    }

    public async Task<Reaction> CreateReactionAsync(Reaction reaction)
    {
        _context.Reactions.Add(reaction);
        await _context.SaveChangesAsync();
        return reaction;
    }

    public async Task DeleteReactionAsync(int id)
    {
        var reaction = await _context.Reactions.FindAsync(id);
        if (reaction != null)
        {
            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetLikeCountAsync(int? postId, int? commentId)
    {
        return await _context.Reactions
            .CountAsync(r => r.Type == ReactionType.Like
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId)));
    }

    public async Task<List<Reaction>> GetReactionsForPostAsync(int postId)
    {
        return await _context.Reactions
            .Where(r => r.PostId == postId)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<List<Reaction>> GetReactionsForCommentAsync(int commentId)
    {
        return await _context.Reactions
            .Where(r => r.CommentId == commentId)
            .Include(r => r.User)
            .ToListAsync();
    }
}
