using HellavorX.Models;
using HellavorX.ViewModels;

namespace HellavorX.Services;

public interface IPostService
{
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task<List<Post>> GetPostsByUserIdAsync(string userId);
    Task<List<Post>> GetFeedForUserAsync(string userId);
    Task<List<Post>> SearchPostsAsync(string query);
    Task<Post> CreatePostAsync(string content, string userId, List<SelectedFile> files);
    Task UpdatePostAsync(int postId, string content, string userId);
    Task DeletePostAsync(int postId, string userId);
}
