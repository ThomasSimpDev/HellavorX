using HellavorX.Data;
using HellavorX.Models;
using HellavorX.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Services;

public class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepository;

    public ReactionService(IReactionRepository reactionRepository)
    {
        _reactionRepository = reactionRepository;
    }

    public async Task ToggleReactionAsync(int? postId, int? commentId, string userId, ReactionType type)
    {
        // Use single context for entire transaction to ensure consistency
        await _reactionRepository.ToggleReactionAsync(postId, commentId, userId, type);
    }

    public async Task<int> GetReactionCountAsync(int? postId, int? commentId, ReactionType type)
    {
        return await _reactionRepository.GetReactionCountAsync(postId, commentId, type);
    }

    public async Task<ReactionType?> GetUserReactionAsync(int? postId, int? commentId, string userId)
    {
        return await _reactionRepository.GetUserReactionAsync(postId, commentId, userId);
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

