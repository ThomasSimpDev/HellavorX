using HellavorX.Data;
using HellavorX.Models;
using Microsoft.EntityFrameworkCore;

namespace HellavorX.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.MediaFiles)
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await _context.Comments.FindAsync(id);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task UpdateCommentAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteCommentWithRepliesAsync(int commentId, int postId)
    {
        var allComments = await _context.Comments.Where(c => c.PostId == postId).ToListAsync();
        DeleteCommentAndRepliesRecursive(commentId, allComments);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private void DeleteCommentAndRepliesRecursive(int commentId, List<Comment> allComments)
    {
        var replies = allComments.Where(c => c.ParentCommentId == commentId).ToList();
        foreach (var reply in replies)
        {
            DeleteCommentAndRepliesRecursive(reply.Id, allComments);
        }
        var comment = allComments.FirstOrDefault(c => c.Id == commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
        }
    }
}
