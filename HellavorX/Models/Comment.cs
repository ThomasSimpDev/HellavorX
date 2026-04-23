namespace HellavorX.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    
    public int ReactionCount => Reactions.Count;
    public Dictionary<ReactionType, int> ReactionCounts => Reactions.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count());
}
