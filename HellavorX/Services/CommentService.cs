using HellavorX.Data;
using HellavorX.Models;
using HellavorX.Repositories;
using Microsoft.AspNetCore.Components.Forms;

namespace HellavorX.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly FileUploadService _fileUploadService;
    private readonly ApplicationDbContext _context;

    public CommentService(ICommentRepository commentRepository, FileUploadService fileUploadService, ApplicationDbContext context)
    {
        _commentRepository = commentRepository;
        _fileUploadService = fileUploadService;
        _context = context;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
        
        // Build hierarchy
        foreach (var comment in comments)
        {
            comment.Replies = comments.Where(c => c.ParentCommentId == comment.Id).ToList();
        }
        
        return comments;
    }

    public async Task<Comment> CreateCommentAsync(string content, string userId, int postId, int? parentId, List<IBrowserFile> files)
    {
        var comment = new Comment
        {
            Content = content,
            UserId = userId,
            PostId = postId,
            ParentCommentId = parentId,
            CreatedAt = DateTime.UtcNow
        };

        var createdComment = await _commentRepository.CreateCommentAsync(comment);

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
                CommentId = createdComment.Id
            };

            _context.MediaFiles.Add(media);
        }

        await _context.SaveChangesAsync();
        return createdComment;
    }

    public async Task UpdateCommentAsync(int commentId, string content, string userId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment != null && comment.UserId == userId)
        {
            comment.Content = content;
            await _commentRepository.UpdateCommentAsync(comment);
        }
    }

    public async Task DeleteCommentAsync(int commentId, string userId, int postId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment != null && comment.UserId == userId)
        {
            await _commentRepository.DeleteCommentWithRepliesAsync(commentId, postId);
        }
    }
}
