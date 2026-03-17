using HellavorX.Models;
using HellavorX.Services;
using HellavorX.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace HellavorX.Components;

public partial class CommentThread
{
    [Parameter] public Comment Comment { get; set; } = null!;
    [Parameter] public int PostId { get; set; }
    [Parameter] public Func<Task> OnReplySubmit { get; set; } = null!;
    [Parameter] public Action<Comment>? OnReplyClick { get; set; }
    [Parameter] public bool IsReply { get; set; }

    [Inject] private ICommentService CommentService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool isEditing;
    private EditPostViewModel editModel = new();
    private string? currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        currentUserId = UserManager.GetUserId(user);
    }

    private void BeginEdit()
    {
        if (Comment.UserId != currentUserId) return;
        isEditing = true;
        editModel.Content = Comment.Content;
    }

    private void CancelEdit()
    {
        isEditing = false;
        editModel = new();
    }

    private async Task SaveEdit()
    {
        await CommentService.UpdateCommentAsync(Comment.Id, editModel.Content, currentUserId!);
        await OnReplySubmit();
        CancelEdit();
    }

    private async Task ConfirmDelete()
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this comment?"))
        {
            await CommentService.DeleteCommentAsync(Comment.Id, currentUserId!, PostId);
            await OnReplySubmit();
        }
    }
}
