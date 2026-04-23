using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HellavorX.Models;

public class Reaction
{
    public int Id { get; set; }
    
    public int? PostId { get; set; }
    [ForeignKey("PostId")]
    public Post? Post { get; set; }
    
    public int? CommentId { get; set; }
    [ForeignKey("CommentId")]
    public Comment? Comment { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
