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
    private IBrowserFile? selectedProfilePic;
    private string? currentUserId;
    private int? editPostId;
    private EditPostViewModel editModelPost = new();

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

    private void HandleProfilePicture(InputFileChangeEventArgs e)
    {
        selectedProfilePic = e.File;
    }

    private async Task UpdateProfile()
    {
        if (user == null || !isOwnProfile)
            return;

        isUpdating = true;
        await UserService.UpdateUserProfileAsync(currentUserId!, editModel.Name, editModel.Bio, selectedProfilePic);
        
        showEditForm = false;
        isUpdating = false;
        selectedProfilePic = null;
        await LoadProfile();
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
