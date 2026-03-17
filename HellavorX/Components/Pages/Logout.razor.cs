using HellavorX.Services;
using Microsoft.AspNetCore.Components;

namespace HellavorX.Components.Pages;

public partial class Logout
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}
