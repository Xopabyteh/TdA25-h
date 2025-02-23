using Blazored.SessionStorage;
using h.Client.Services;
using h.Contracts.Matchmaking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game.Multiplayer;

public partial class MatchmakingQueue : IAsyncDisposable
{
    private readonly IHApiClient _api;
    private readonly NavigationManager _navigationManager;
    private readonly ISessionStorageService _sessionStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IWasmCurrentUserStateService _currentUserStateService;

    private HubConnection? hubConnection;
    private FoundMatchingDetailsResponse? currentMatching;
    private FoundMatchingPlayerDetailDto otherPlayer => currentMatching!.Value.Player1.PlayerId 
            == _currentUserStateService.UserDetails!.Value.Uuid
        ? currentMatching!.Value.Player2
        : currentMatching!.Value.Player1;

    private bool isJoinedQueue;

    public MatchmakingQueue(
        IHApiClient api,
        NavigationManager navigationManager,
        ISessionStorageService sessionStorage,
        AuthenticationStateProvider authStateProvider,
        IAuthorizationService authorizationService,
        IWasmCurrentUserStateService currentUserStateService)
    {
        _api = api;
        _navigationManager = navigationManager;
        _sessionStorage = sessionStorage;
        _authStateProvider = authStateProvider;
        _authorizationService = authorizationService;
        _currentUserStateService = currentUserStateService;
    }

    protected override async Task OnInitializedAsync()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;
    
        await _currentUserStateService.EnsureStateAsync();

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var canJoinMatch = await _authorizationService.AuthorizeAsync(authState.User, nameof(AppPolicies.AbleToJoinMatchmaking));
        if(!canJoinMatch.Succeeded)
            return;
        
        // 1. Join to hub
        // 2. Join queue
        hubConnection = CreateHubWithCallbacks();
        await hubConnection.StartAsync();

        await HandleJoinQueue();
    }

    private HubConnection CreateHubWithCallbacks()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_navigationManager}{IMatchmakingHubClient.Route}")
            .Build();

        hubConnection.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), async response =>
        {
            // Set matching
            currentMatching = response;

            // Todo: start countdown for automatic cancellation

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<Guid>(nameof(IMatchmakingHubClient.MatchCancelled), async matchId =>
        {
            if (currentMatching is null)
                return;

            if (matchId != currentMatching!.Value.MatchId)
                throw new Exception("Match cancelled does not match current match");

            currentMatching = null;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<Guid>(nameof(IMatchmakingHubClient.PlayerAccepted), async playerId =>
        {
        });

        hubConnection.On<Guid>(nameof(IMatchmakingHubClient.NewGameSessionCreated), async gameId =>
        {
            await _sessionStorage.SetItemAsync(MultiplayerGame.GameIdSessionStorageKey, gameId);

            _navigationManager.NavigateTo(PageRoutes.Multiplayer.MultiplayerGame);
        });

        return hubConnection;
    }

    public async Task HandleAcceptMatching()
    {
        if (currentMatching is null)
            return;

        await _api.AcceptMatch(currentMatching!.Value.MatchId);
    }

    public async Task HandleDeclineMatching()
    {
        if (currentMatching is null)
            return;

        await _api.DeclineMatch(currentMatching!.Value.MatchId);    
    }

    public async Task HandleJoinQueue()
    {
        await _api.JoinMatchmaking();
        
        isJoinedQueue = true;
    }

    public async Task HandleLeaveQueue()
    {
        await _api.LeaveMatchmaking();
        
        isJoinedQueue = false;
    }

    public ValueTask DisposeAsync()
    {
        if(hubConnection is not null)
            return hubConnection.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
