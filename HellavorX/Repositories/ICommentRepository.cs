using HellavorX.Models;

namespace HellavorX.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task UpdateCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
    Task DeleteCommentWithRepliesAsync(int commentId, int postId);
}
