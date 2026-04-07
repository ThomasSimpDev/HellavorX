using HellavorX.Models;
using HellavorX.Services;
using HellavorX.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace HellavorX.Components.Pages;

public partial class PostDetail
{
    [Parameter] public int Id { get; set; }
    
    [Inject] private IPostService PostService { get; set; } = default!;
    [Inject] private ICommentService CommentService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private Post? post;
    private CreateCommentViewModel newComment = new();
    private List<SelectedFile> selectedCommentFiles = new();
    private bool isCommenting;
    private string? currentUserId;
    private Comment? replyingTo;
    private bool isEditing;
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
        await LoadPost();
    }

    public async Task LoadPost()
    {
        post = await PostService.GetPostByIdAsync(Id);
        
        if (post != null)
        {
            var comments = await CommentService.GetCommentsByPostIdAsync(Id);
            post.Comments = comments;
        }

        StateHasChanged();
    }

    private void BeginEdit()
    {
        if (post == null || post.UserId != currentUserId) return;
        isEditing = true;
        editModel.Content = post.Content;
    }

    private void CancelEdit()
    {
        isEditing = false;
        editModel = new();
    }

    private async Task SaveEdit()
    {
        if (post == null) return;
        await PostService.UpdatePostAsync(post.Id, editModel.Content, currentUserId!);
        await LoadPost();
        CancelEdit();
    }

    private async Task ConfirmDelete()
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this post?"))
        {
            await PostService.DeletePostAsync(Id, currentUserId!);
            Navigation.NavigateTo("/");
        }
    }

    private async Task HandleCommentFileSelection(InputFileChangeEventArgs e)
    {
        var incoming = e.GetMultipleFiles(10 - selectedCommentFiles.Count);
        foreach (var file in incoming)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            
            selectedCommentFiles.Add(new SelectedFile
            {
                Name = file.Name,
                Content = bytes,
                ContentType = file.ContentType
            });
        }
    }

    public void HandleReplyClick(Comment comment)
    {
        replyingTo = comment;
        StateHasChanged();
    }

    private void CancelReply()
    {
        replyingTo = null;
    }

    private async Task AddComment(int? parentId)
    {
        if (string.IsNullOrWhiteSpace(newComment.Content) && !selectedCommentFiles.Any())
            return;

        isCommenting = true;
        await CommentService.CreateCommentAsync(newComment.Content, currentUserId!, Id, parentId, selectedCommentFiles);

        newComment = new();
        selectedCommentFiles.Clear();
        replyingTo = null;
        await LoadPost();
        isCommenting = false;
    }
}
