using HellavorX.Models;

namespace HellavorX.Repositories;

public interface IPostRepository
{
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task<List<Post>> GetPostsByUserIdAsync(string userId);
    Task<Post> CreatePostAsync(Post post);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(int id);
}
