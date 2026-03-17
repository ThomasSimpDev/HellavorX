using HellavorX.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public interface ICommentService
{
    Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<Comment> CreateCommentAsync(string content, string userId, int postId, int? parentId, List<IBrowserFile> files);
    Task UpdateCommentAsync(int commentId, string content, string userId);
    Task DeleteCommentAsync(int commentId, string userId, int postId);
}
