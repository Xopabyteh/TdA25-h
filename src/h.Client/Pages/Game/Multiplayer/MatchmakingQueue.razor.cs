using Blazored.SessionStorage;
using h.Client.Services;
using h.Contracts.Matchmaking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
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

    private IDisposable? locationChangedEventDisposable;
    private HubConnection? hubConnection;
    private FoundMatchingDetailsResponse? currentMatching;
    private List<Guid> currentMatchingAcceptees = new(2);

    private FoundMatchingPlayerDetailDto otherPlayer => currentMatching!.Value.Player1.PlayerId 
            == _currentUserStateService.UserDetails!.Value.Uuid
        ? currentMatching!.Value.Player2
        : currentMatching!.Value.Player1;

    private bool isJoinedQueue;

    // Queue statistics - how many players are in queue and position in queue
    private int? positionInQueue;
    private int? totalPlayersInQueue;
    private const int QueueRefreshIntervalMS = 5000;
    private Timer? refreshQueueStatisticsTimer;

    private bool isLeavePopupVisible = false;

    private Timer? acceptCountdownTimer;
    private bool isCountingDown = false;
    private double acceptProgress = 100;
    
    private bool isPageFirstLoaded;

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
        
        // Join hub
        hubConnection = CreateHubWithCallbacks();
        await hubConnection.StartAsync();

        // Leave on location change
        locationChangedEventDisposable = _navigationManager.RegisterLocationChangingHandler(HandleLocationChanged);

        // Start queue statistics refresh
        refreshQueueStatisticsTimer = new Timer(
            async _ => await RefreshQueueStatistics(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(QueueRefreshIntervalMS)
        );

        // Join queue on load
        await HandleJoinQueue();

        isPageFirstLoaded = true;
    }

    /// <summary>
    /// Sets progress bar percentage, shown in UI.
    /// Also, after countdown ends tries to cancel match.
    /// </summary>
    /// <param name="endTime"></param>
    private void StartAcceptMatchProgressBarCountdown(DateTimeOffset endTime)
    {
        var startTime = DateTime.UtcNow;
        var totalDurationMs = (endTime - startTime).TotalMilliseconds;

        isCountingDown = true;
        acceptCountdownTimer = new Timer(
            async _ =>
            {
                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                acceptProgress = Math.Max(100 - (elapsedMs / totalDurationMs * 100), 0);

                if (acceptProgress <= 0)
                {
                    acceptCountdownTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    isCountingDown = false;
                }

                await InvokeAsync(StateHasChanged); // Update UI without blocking
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    private void StopProgressBarCountdown()
    {
        acceptCountdownTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        isCountingDown = false;
    }

    private async Task RefreshQueueStatistics()
    {
        if(isJoinedQueue && hubConnection is not null)
        {
            positionInQueue = await hubConnection.InvokeAsync<int>("GetPositionInQueue");
        }

        totalPlayersInQueue = await _api.GetQueueSize();

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Try leave queue, so doesn't block...
    /// </summary>
    private async ValueTask HandleLocationChanged(LocationChangingContext ctx)
    {
        if(currentMatching is not null)
        {
            await HandleDeclineMatching();
        }

        if (isJoinedQueue)
        {
            await HandleLeaveQueue();
        }
    }

    private HubConnection CreateHubWithCallbacks()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_navigationManager.BaseUri}{IMatchmakingHubClient.Route}")
            .Build();

        hubConnection.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), async response =>
        {
            // Set matching
            currentMatching = response;

            // We are no longer in queue
            isJoinedQueue = false;

            // Start countdown
            StartAcceptMatchProgressBarCountdown(response.ExpiresAt);

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<MatchCancelledResponse>(nameof(IMatchmakingHubClient.MatchCancelled), async response =>
        {
            if (currentMatching is null)
                return;

            if (response.MatchId != currentMatching!.Value.MatchId)
                throw new Exception("Match cancelled does not match current match");

            StopProgressBarCountdown();
            CancelMatching(response.NewPositionInQueue);

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<Guid>(nameof(IMatchmakingHubClient.PlayerAccepted), async playerId =>
        {
            currentMatchingAcceptees.Add(playerId);
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<Guid>(nameof(IMatchmakingHubClient.NewGameSessionCreated), async gameId =>
        {
            await _sessionStorage.SetItemAsync(MultiplayerGame.GameIdSessionStorageKey, gameId);

            _navigationManager.NavigateTo(PageRoutes.Multiplayer.MultiplayerGame);
        });

        return hubConnection;
    }

    private void CancelMatching(int? newPositionInQueue)
    {
        currentMatching = null;
        currentMatchingAcceptees.Clear();

        if(newPositionInQueue is null)
        {
            // No longer in queue
            isJoinedQueue = false;
        }
        positionInQueue = newPositionInQueue;
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
        var response = await _api.JoinMatchmaking();

        isJoinedQueue = true;
        positionInQueue = response;
        
        // Make timer click
        refreshQueueStatisticsTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(QueueRefreshIntervalMS));
    }

    public async Task HandleLeaveQueue()
    {
        isLeavePopupVisible = false; // Incase it was open
        positionInQueue = null;

        await _api.LeaveMatchmaking();
        
        isJoinedQueue = false;
    }

    public async ValueTask DisposeAsync()
    {
        locationChangedEventDisposable?.Dispose();

        if (acceptCountdownTimer is not null)
            await acceptCountdownTimer.DisposeAsync();

        if (refreshQueueStatisticsTimer is not null)
            await refreshQueueStatisticsTimer.DisposeAsync();

        if (hubConnection is not null)
            await hubConnection.DisposeAsync();
    }

    public async Task HandleLoginRedirect() => _navigationManager.NavigateTo(PageRoutes.Auth.LoginIndexWithQuery(_navigationManager.RelativeUri()));

    public void HandleCloseLeavePopup()
    {
        isLeavePopupVisible = false;
    }

    public void HandleOpenLeavePopup()
    {
        isLeavePopupVisible = true;
    }
}
