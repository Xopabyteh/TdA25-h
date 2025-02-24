using h.Client.Pages;
using h.Server.Entities.Users;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace h.Server.Components.Layout;
public partial class MainLayout
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    protected User? currentUser;
    protected bool isGuest;

    public MainLayout(IDbContextFactory<AppDbContext> dbContextFactory, AuthenticationStateProvider authenticationStateProvider)
    {
        _dbContextFactory = dbContextFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task OnInitializedAsync()
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
            return;

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if(authState.User is not {Identity: { IsAuthenticated: true } })
            return;

        var guestId = authState.User.GetGuestId();
        if(guestId is not null)
        {
            isGuest = true;
            return;
        }

        var userId = authState.User.GetUserId();

        await using var db = _dbContextFactory.CreateDbContext();
        currentUser = await db.UsersDbSet.FirstAsync(u => u.Uuid == userId);
    }
}