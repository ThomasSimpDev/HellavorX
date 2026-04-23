using HellavorX.Models;

namespace HellavorX.Repositories;

public interface IReactionRepository
{
    Task<Reaction?> GetReactionAsync(int? postId, int? commentId, string userId, ReactionType type);
    Task<Reaction> CreateReactionAsync(Reaction reaction);
    Task DeleteReactionAsync(int id);
    Task<int> GetLikeCountAsync(int? postId, int? commentId);
    Task<List<Reaction>> GetReactionsForPostAsync(int postId);
    Task<List<Reaction>> GetReactionsForCommentAsync(int commentId);
}
