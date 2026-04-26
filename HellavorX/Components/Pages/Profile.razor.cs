using HellavorX.Models;
using HellavorX.Services;
using HellavorX.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace HellavorX.Components.Pages;

public partial class Profile
{
    [Parameter] public string Username { get; set; } = string.Empty;
    
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IPostService PostService { get; set; } = default!;
    [Inject] private IFollowService FollowService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private ApplicationUser? user;
    private List<Post> userPosts = new();
    private bool isOwnProfile;
    private bool showEditForm;
    private bool isUpdating;
    private EditProfileViewModel editModel = new();
    private SelectedFile? selectedProfilePic;
    private string? currentUserId;
    private int? editPostId;
    private EditPostViewModel editModelPost = new();
    private bool isFollowing;
    private int followerCount;
    private int followingCount;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var authUser = authState.User;
        
        if (!authUser.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        currentUserId = UserManager.GetUserId(authUser);
        
        // If no username provided in URL, get current user's username
        if (string.IsNullOrEmpty(Username))
        {
            var currentUser = await UserManager.GetUserAsync(authUser);
            if (currentUser != null)
            {
                Username = currentUser.UserName ?? "";
            }
        }
        
        await LoadProfile();
    }

    private async Task LoadProfile()
    {
        if (string.IsNullOrEmpty(Username))
            return;
            
        user = await UserService.GetUserByUsernameAsync(Username);
        
        if (user == null)
            return;

        isOwnProfile = user.Id == currentUserId;
        userPosts = await PostService.GetPostsByUserIdAsync(user.Id);

        if (!isOwnProfile && currentUserId != null)
        {
            isFollowing = await FollowService.IsFollowingAsync(currentUserId, user.Id);
        }

        followerCount = await FollowService.GetFollowerCountAsync(user.Id);
        followingCount = await FollowService.GetFollowingCountAsync(user.Id);

        editModel = new EditProfileViewModel
        {
            Name = user.Name,
            Bio = user.Bio
        };
    }

    private void ToggleEdit()
    {
        showEditForm = !showEditForm;
    }

    private async Task ToggleFollow()
    {
        if (user == null || currentUserId == null || isOwnProfile)
            return;

        if (isFollowing)
        {
            await FollowService.UnfollowUserAsync(currentUserId, user.Id);
            isFollowing = false;
            followerCount--;
        }
        else
        {
            await FollowService.FollowUserAsync(currentUserId, user.Id);
            isFollowing = true;
            followerCount++;
        }
    }

    private async Task HandleProfilePicture(InputFileChangeEventArgs e)
    {
        var file = e.File;
        using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        
        selectedProfilePic = new SelectedFile
        {
            Name = file.Name,
            Content = bytes,
            ContentType = file.ContentType
        };
    }

    private async Task UpdateProfile()
    {
        if (user == null || !isOwnProfile)
            return;

        isUpdating = true;
        StateHasChanged(); // Force UI to update to show loading state
        
        await UserService.UpdateUserProfileAsync(currentUserId!, editModel.Name, editModel.Bio, selectedProfilePic);
        
        showEditForm = false;
        isUpdating = false;
        selectedProfilePic = null;
        await LoadProfile();
        StateHasChanged(); // Force UI to update with new profile data
    }

    private void StartEdit(Post post)
    {
        if (!isOwnProfile) return;
        editPostId = post.Id;
        editModelPost.Content = post.Content;
    }

    private void CancelEdit()
    {
        editPostId = null;
        editModelPost = new();
    }

    private async Task SaveEdit(int postId)
    {
        await PostService.UpdatePostAsync(postId, editModelPost.Content, currentUserId!);
        await LoadProfile();
        CancelEdit();
    }

    private async Task DeletePost(int postId)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Delete this post?"))
            return;

        await PostService.DeletePostAsync(postId, currentUserId!);
        await LoadProfile();
    }
}
