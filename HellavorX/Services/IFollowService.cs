namespace HellavorX.Services;

public interface IFollowService
{
    Task<bool> IsFollowingAsync(string followerId, string followingId);
    Task FollowUserAsync(string followerId, string followingId);
    Task UnfollowUserAsync(string followerId, string followingId);
    Task<int> GetFollowerCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
    Task<List<string>> GetFollowingIdsAsync(string userId);
}

