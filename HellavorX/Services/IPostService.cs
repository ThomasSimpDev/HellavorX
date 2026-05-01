using HellavorX.Models;
using HellavorX.ViewModels;

namespace HellavorX.Services;

public interface IPostService
{
    Task<List<Post>> GetAllPostsAsync(int skip = 0, int take = 20);
    Task<Post?> GetPostByIdAsync(int id);
    Task<List<Post>> GetPostsByUserIdAsync(string userId, int skip = 0, int take = 20);
    Task<List<Post>> GetFeedForUserAsync(string userId, int skip = 0, int take = 20);
    Task<List<Post>> SearchPostsAsync(string query, int skip = 0, int take = 20);
    Task<Post> CreatePostAsync(string content, string userId, List<SelectedFile> files);
    Task UpdatePostAsync(int postId, string content, string userId);
    Task DeletePostAsync(int postId, string userId);
}
