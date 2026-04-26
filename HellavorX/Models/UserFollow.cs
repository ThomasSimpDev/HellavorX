namespace HellavorX.Models;

public class UserFollow
{
    public string FollowerId { get; set; } = string.Empty;
    public ApplicationUser Follower { get; set; } = null!;

    public string FollowingId { get; set; } = string.Empty;
    public ApplicationUser Following { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

