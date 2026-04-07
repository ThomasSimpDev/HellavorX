using HellavorX.Models;
using HellavorX.Services;
using HellavorX.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace HellavorX.Components.Pages;

public partial class Home
{
    [Inject] private IPostService PostService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected IHttpContextAccessor? HttpContextAccessor { get; set; }
    [Inject] protected IAntiforgery? Antiforgery { get; set; }

    private List<Post> posts = new();
    private CreatePostViewModel newPost = new();
    private List<SelectedFile> selectedFiles = new();
    private bool isPosting;
    private string? currentUserId;
    private int? editPostId;
    private EditPostViewModel editModel = new();

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

    private async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        var incoming = e.GetMultipleFiles(10 - selectedFiles.Count);
        foreach (var file in incoming)
        {
            // Read file immediately since IBrowserFile stream may not be available later
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            selectedFiles.Add(new SelectedFile
            {
                Name = file.Name,
                Content = bytes,
                ContentType = file.ContentType
            });
        }
        StateHasChanged();
    }

    private void RemoveFile(SelectedFile file)
    {
        selectedFiles.Remove(file);
    }

    private async Task CreatePost()
    {
        if (string.IsNullOrWhiteSpace(newPost.Content) && !selectedFiles.Any())
            return;

        isPosting = true;
        await PostService.CreatePostAsync(newPost.Content, currentUserId!, selectedFiles);

        newPost = new();
        selectedFiles.Clear();
        await LoadPosts();
        isPosting = false;
    }

    private void StartEdit(Post post)
    {
        if (post.UserId != currentUserId) return;
        editPostId = post.Id;
        editModel.Content = post.Content;
    }

    private void CancelEdit()
    {
        editPostId = null;
        editModel = new();
    }

    private async Task SaveEdit(int postId)
    {
        await PostService.UpdatePostAsync(postId, editModel.Content, currentUserId!);
        await LoadPosts();
        CancelEdit();
    }

    private async Task DeletePost(int postId)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "Delete this post?"))
            return;

        await PostService.DeletePostAsync(postId, currentUserId!);
        await LoadPosts();
    }
}
