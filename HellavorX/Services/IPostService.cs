using HellavorX.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public interface IPostService
{
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task<List<Post>> GetPostsByUserIdAsync(string userId);
    Task<Post> CreatePostAsync(string content, string userId, List<IBrowserFile> files);
    Task UpdatePostAsync(int postId, string content, string userId);
    Task DeletePostAsync(int postId, string userId);
}
