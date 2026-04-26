using HellavorX.Models;
using HellavorX.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;

namespace HellavorX.Components.Pages;

public partial class Search
{
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IPostService PostService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery] public string? Query { get; set; }

    private string searchQuery = string.Empty;
    private List<ApplicationUser> userResults = new();
    private List<Post> postResults = new();
    private bool isSearching;
    private bool hasSearched;
    private string activeTab = "users";

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Query))
        {
            searchQuery = Query;
            await ExecuteSearch();
        }
    }

    private async Task ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return;

        isSearching = true;
        hasSearched = true;

        var userTask = UserService.SearchUsersAsync(searchQuery);
        var postTask = PostService.SearchPostsAsync(searchQuery);

        await Task.WhenAll(userTask, postTask);

        userResults = await userTask;
        postResults = await postTask;

        isSearching = false;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await ExecuteSearch();
        }
    }
}

