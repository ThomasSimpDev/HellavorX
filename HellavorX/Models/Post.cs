namespace HellavorX.Models;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
