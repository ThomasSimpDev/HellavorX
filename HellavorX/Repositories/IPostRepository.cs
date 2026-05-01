using HellavorX.Models;

namespace HellavorX.Repositories;

public interface IPostRepository
{
    Task<List<Post>> GetAllPostsAsync(int skip = 0, int take = 20);
    Task<Post?> GetPostByIdAsync(int id);
    Task<List<Post>> GetPostsByUserIdAsync(string userId, int skip = 0, int take = 20);
    Task<List<Post>> GetFeedForUserAsync(string userId, List<string> followingIds, int skip = 0, int take = 20);
    Task<List<Post>> SearchPostsAsync(string query, int skip = 0, int take = 20);
    Task<Post> CreatePostAsync(Post post);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(int id);
}
