using HellavorX.Models;

namespace HellavorX.Services;

public interface IReactionService
{
    Task ToggleReactionAsync(int? postId, int? commentId, string userId, ReactionType type);
    Task<int> GetReactionCountAsync(int? postId, int? commentId, ReactionType type);
    Task<ReactionType?> GetUserReactionAsync(int? postId, int? commentId, string userId);
    Task LoadReactionsAsync(Post post);
    Task LoadReactionsAsync(Comment comment);
}
