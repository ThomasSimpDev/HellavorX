using HellavorX.Repositories;

namespace HellavorX.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;

    public FollowService(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        return _followRepository.IsFollowingAsync(followerId, followingId);
    }

    public Task FollowUserAsync(string followerId, string followingId)
    {
        return _followRepository.FollowUserAsync(followerId, followingId);
    }

    public Task UnfollowUserAsync(string followerId, string followingId)
    {
        return _followRepository.UnfollowUserAsync(followerId, followingId);
    }

    public Task<int> GetFollowerCountAsync(string userId)
    {
        return _followRepository.GetFollowerCountAsync(userId);
    }

    public Task<int> GetFollowingCountAsync(string userId)
    {
        return _followRepository.GetFollowingCountAsync(userId);
    }

    public Task<List<string>> GetFollowingIdsAsync(string userId)
    {
        return _followRepository.GetFollowingIdsAsync(userId);
    }
}

