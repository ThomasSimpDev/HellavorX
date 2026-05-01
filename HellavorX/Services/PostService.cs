using HellavorX.Data;
using HellavorX.Models;
using HellavorX.Repositories;
using HellavorX.ViewModels;
using Microsoft.Extensions.Logging;

namespace HellavorX.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly SupabaseStorageService _fileUploadService;
    private readonly ApplicationDbContext _context;
    private readonly IFollowService _followService;
    private readonly ILogger<PostService>? _logger;

    public PostService(IPostRepository postRepository, SupabaseStorageService fileUploadService, ApplicationDbContext context, IFollowService followService, ILogger<PostService>? logger = null)
    {
        _postRepository = postRepository;
        _fileUploadService = fileUploadService;
        _context = context;
        _followService = followService;
        _logger = logger;
    }

public async Task<List<Post>> GetAllPostsAsync(int skip = 0, int take = 20)
    {
        return await _postRepository.GetAllPostsAsync(skip, take);
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _postRepository.GetPostByIdAsync(id);
    }

    public async Task<List<Post>> GetPostsByUserIdAsync(string userId, int skip = 0, int take = 20)
    {
        return await _postRepository.GetPostsByUserIdAsync(userId, skip, take);
    }

    public async Task<List<Post>> GetFeedForUserAsync(string userId, int skip = 0, int take = 20)
    {
        var followingIds = await _followService.GetFollowingIdsAsync(userId);
        return await _postRepository.GetFeedForUserAsync(userId, followingIds, skip, take);
    }

    public async Task<List<Post>> SearchPostsAsync(string query, int skip = 0, int take = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Post>();
        return await _postRepository.SearchPostsAsync(query, skip, take);
    }

    public async Task<Post> CreatePostAsync(string content, string userId, List<SelectedFile> files)
    {
        // Check if files are being uploaded but storage is not available
        if (files.Count > 0 && !_fileUploadService.IsAvailable)
        {
            _logger?.LogWarning("User {UserId} attempted to create a post with {FileCount} files but Supabase storage is not available", userId, files.Count);
            throw new InvalidOperationException("File uploads are currently unavailable. Please try again later or contact support.");
        }

        var post = new Post
        {
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPost = await _postRepository.CreatePostAsync(post);

        foreach (var file in files)
        {
            using var stream = new MemoryStream(file.Content);
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
