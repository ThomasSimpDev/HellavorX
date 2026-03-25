using HellavorX.Data;
using HellavorX.Models;
using HellavorX.Repositories;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly SupabaseStorageService _fileUploadService;
    private readonly ApplicationDbContext _context;

    public PostService(IPostRepository postRepository, SupabaseStorageService fileUploadService, ApplicationDbContext context)
    {
        _postRepository = postRepository;
        _fileUploadService = fileUploadService;
        _context = context;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _postRepository.GetAllPostsAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _postRepository.GetPostByIdAsync(id);
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(string userId)
    {
        return await _postRepository.GetPostsByUserIdAsync(userId);
    }

    public async Task<Post> CreatePostAsync(string content, string userId, List<IBrowserFile> files)
    {
        var post = new Post
        {
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPost = await _postRepository.CreatePostAsync(post);

        foreach (var file in files)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            var url = await _fileUploadService.UploadFileAsync(stream, file.Name);
            var mediaType = _fileUploadService.GetMediaType(file.Name);

            var media = new Media
            {
                Url = url,
                Type = mediaType,
                FileName = file.Name,
                PostId = createdPost.Id
            };

            _context.MediaFiles.Add(media);
        }

        await _context.SaveChangesAsync();
        return createdPost;
    }

    public async Task UpdatePostAsync(int postId, string content, string userId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null && post.UserId == userId)
        {
            post.Content = content;
            await _postRepository.UpdatePostAsync(post);
        }
    }

    public async Task DeletePostAsync(int postId, string userId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null && post.UserId == userId)
        {
            await _postRepository.DeletePostAsync(postId);
        }
    }
}
