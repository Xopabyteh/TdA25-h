using Blazored.SessionStorage;
using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Primitives.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game.Multiplayer;

// Todo: client prediction and server reconciliation? (for smoother experience)
public partial class MultiplayerGame : IAsyncDisposable
{
    public const string GameIdSessionStorageKey = "h.multiplayerSession.gameId";
    private const int ClockUpdateIntervalMs = 333;

    private readonly ISessionStorageService _sessionStorageService;
    private readonly NavigationManager _navigationManager;

    private Guid gameId;
    private HubConnection? hubConnection;

    /// <summary>
    /// After all players are loaded, the game will start and field can be shown
    /// </summary>
    private bool isGameStarted;
    private MultiplayerGameStartedResponse gameDetails;
    private bool isGameEnded;

    private int moveI = 1;
    private int TurnI => moveI / 2;

    private MultiplayerGameSessionUserDetailDTO ourPlayer;
    /// <summary>
    /// Access field by [y,x] (y is row, x is column)
    /// </summary>
    private GameSymbol[,] gameField = new GameSymbol[15,15]; // Todo: remove magic numbers? (throughout the entire app lol)
    /// <summary>
    /// Key: session id,
    /// Value: user details
    /// </summary>
    private Dictionary<Guid, MultiplayerGameSessionUserDetailDTO> sessionIdToPlayer;
    private MultiplayerGameSessionUserDetailDTO playerOnTurn;
    private bool areWeOnTurn => playerOnTurn.Identity.SessionId == gameDetails.MySessionId;

    /// <summary>
    /// Key: session id,
    /// Value: players stopwatch
    /// </summary>
    private Dictionary<Guid, TimeSpan> playerClockRemainingTimes;
    private Timer? clockTimer;
    private string GetRemainingClockTimeFormatted(Guid sessionid)
        => playerClockRemainingTimes[sessionid].ToString(@"mm\:ss");

    private string GetClockCss(Guid sessionid)
        => sessionid == playerOnTurn.Identity.SessionId ? "turn" : "";

    public MultiplayerGame(ISessionStorageService sessionStorageService, NavigationManager navigationManager)
    {
        _sessionStorageService = sessionStorageService;
        _navigationManager = navigationManager;
    }

    protected override async Task OnInitializedAsync()
    {
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;
        gameId = await _sessionStorageService.GetItemAsync<Guid>(GameIdSessionStorageKey);

        // 1. Ensure game hub connection
        // 2. Ping we are ready (loaded)
        // 3. Wait for game start
        // 4. Play game

        // Hub connection
        hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_navigationManager.BaseUri}{IMultiplayerGameSessionHubClient.Route}")
            .Build();

        hubConnection.On<MultiplayerGameStartedResponse>(nameof(IMultiplayerGameSessionHubClient.GameStarted), async response =>
        {
            gameDetails = response;

            // Players
            sessionIdToPlayer = response.Players.ToDictionary(
                p => p.Identity.SessionId,
                p => p
            );
            ourPlayer = sessionIdToPlayer[gameDetails.MySessionId];
            playerOnTurn = sessionIdToPlayer[gameDetails.StartingPlayerIdentity.SessionId];

            // Clocks
            playerClockRemainingTimes = response.Players.ToDictionary(
                p => p.Identity.SessionId,
                p => sessionIdToPlayer[p.Identity.SessionId].StartingTimeOnClock);

            clockTimer = CreateClockEatingTimer();

            isGameStarted = true;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            // Update field
            gameField[response.Position.Y, response.Position.X] = response.Symbol;

            // If no more players on turn, game ended, wait for game ended invokation
            if(response.NextPlayerOnTurn is null)
                return;

            // Update turn
            var nextPlayerOnTurnIdentity = response.NextPlayerOnTurn.Value;
            playerOnTurn = gameDetails.Players.First(p => p.Identity.SessionId == nextPlayerOnTurnIdentity.SessionId);
            moveI++;

            // Sync clocks
            foreach (var remainingTimeKvp in response.PlayerRemainingClockTimes)
            {
                playerClockRemainingTimes[remainingTimeKvp.Key] = remainingTimeKvp.Value;
            }

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded), async response =>
        {
            isGameEnded = true;
            if(clockTimer is not null)
            {
                await clockTimer.DisposeAsync();
                clockTimer = null;
            }

            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();

        // Ping we are ready
        await hubConnection.SendAsync("ConfirmLoaded", gameId);
    }

    private async Task HandlePlaceSymbol(int x, int y)
    {
        if(!areWeOnTurn)
            return;

        var placePos = new Int2(x, y);
        await hubConnection!.InvokeAsync("PlaceSymbol", gameDetails.GameId, placePos);
    }

    private Timer CreateClockEatingTimer()
        => new Timer(
                async _ =>
                {
                    if (!isGameStarted)
                        return;

                    if(isGameEnded)
                        return;

                    // Remove elapsed from player on turn
                    // If seconds have changed, update UI
                    var prevSeconds = playerClockRemainingTimes[playerOnTurn.Identity.SessionId].Seconds;
                    var newTime = playerClockRemainingTimes[playerOnTurn.Identity.SessionId] - TimeSpan.FromMilliseconds(ClockUpdateIntervalMs);
                    playerClockRemainingTimes[playerOnTurn.Identity.SessionId] = newTime;

                    if (newTime.Seconds != prevSeconds)
                    {
                        await InvokeAsync(StateHasChanged);
                    }
                },
                null,
                dueTime: TimeSpan.FromSeconds(0),
                period: TimeSpan.FromMilliseconds(ClockUpdateIntervalMs)
            );

    public async ValueTask DisposeAsync()
    {
        if(clockTimer is not null)
            await clockTimer.DisposeAsync();

        if (hubConnection is not null)
            await hubConnection.DisposeAsync();
    }
}