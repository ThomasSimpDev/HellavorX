using HellavorX.Models;
using HellavorX.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace HellavorX.Components.Pages;

public partial class Explore
{
    [Inject] private IPostService PostService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IReactionService ReactionService { get; set; } = default!;

    private List<Post> posts = new();
    private string? currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        currentUserId = UserManager.GetUserId(user);
        await LoadPosts();
    }

    private async Task LoadPosts()
    {
        posts = await PostService.GetAllPostsAsync();
    }

    private async Task TogglePostReaction(int postId, ReactionType type)
    {
        await ReactionService.ToggleReactionAsync(postId, null, currentUserId!, type);
        await LoadPosts();
    }
}

