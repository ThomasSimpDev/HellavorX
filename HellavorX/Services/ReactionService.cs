using HellavorX.Data;
using HellavorX.Models;
using HellavorX.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Services;

public class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepository;
    private readonly ApplicationDbContext _context;

    public ReactionService(IReactionRepository reactionRepository, ApplicationDbContext context)
    {
        _reactionRepository = reactionRepository;
        _context = context;
    }

    public async Task ToggleReactionAsync(int? postId, int? commentId, string userId, ReactionType type)
    {
        // Remove existing reaction from same user on same item if different type
        var existing = await _reactionRepository.GetReactionAsync(postId, commentId, userId, type);
        if (existing != null)
        {
            await _reactionRepository.DeleteReactionAsync(existing.Id);
        }
        else
        {
            // Remove other types from same user
            var otherReactions = await _context.Reactions
                .Where(r => r.UserId == userId 
                    && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId))
                    && r.Type != type)
                .ToListAsync();
            _context.Reactions.RemoveRange(otherReactions);
            
            var reaction = new Reaction
            {
                PostId = postId,
                CommentId = commentId,
                UserId = userId,
                Type = type
            };
            await _reactionRepository.CreateReactionAsync(reaction);
        }
        await _context.SaveChangesAsync();
    }



    public async Task<int> GetReactionCountAsync(int? postId, int? commentId, ReactionType type)
    {
        return await _context.Reactions.CountAsync(r => ((postId.HasValue && r.PostId == postId.Value && !r.CommentId.HasValue) || (commentId.HasValue && r.CommentId == commentId.Value && !r.PostId.HasValue))
            && r.Type == type);
    }

    public async Task<ReactionType?> GetUserReactionAsync(int? postId, int? commentId, string userId)
    {
        var reaction = await _context.Reactions
            .FirstOrDefaultAsync(r => r.UserId == userId 
                && ((postId.HasValue && r.PostId == postId) || (commentId.HasValue && r.CommentId == commentId)));
        return reaction?.Type;
    }

    public async Task LoadReactionsAsync(Post post)
    {
        post.Reactions = await _reactionRepository.GetReactionsForPostAsync(post.Id);
    }

    public async Task LoadReactionsAsync(Comment comment)
    {
        comment.Reactions = await _reactionRepository.GetReactionsForCommentAsync(comment.Id);
    }
}
