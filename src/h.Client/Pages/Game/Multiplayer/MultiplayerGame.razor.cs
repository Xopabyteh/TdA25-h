using Blazored.SessionStorage;
using h.Contracts.MultiplayerGames;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game.Multiplayer;

public partial class MultiplayerGame : IAsyncDisposable
{
    public const string GameIdSessionStorageKey = "h.multiplayerSession.gameId";

    private readonly ISessionStorageService _sessionStorageService;
    private readonly NavigationManager _navigationManager;

    private Guid gameId;
    private HubConnection? hubConnection;

    /// <summary>
    /// After all players are loaded, the game will start and field can be shown
    /// </summary>
    private bool isGameStarted;
    private MultiplayerGameStartedResponse gameDetails;

    private bool xOnTurn = true;
    private string turnDisplaySrc = "";
    private string turnDisplayAlt = "";
    private int turnI = 1;

    private int elo1 = 100;
    private int elo2 = 100;

    private bool isPlayerX = true;


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
            Console.WriteLine($"Game started {response}");
            gameDetails = response;
            isGameStarted = true;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), response =>
        {
            Console.WriteLine($"Player made move {response}");
        });

        hubConnection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded), response =>
        {
            Console.WriteLine($"Game ended {response}");
        });

        await hubConnection.StartAsync();

        // Ping we are ready
        await hubConnection.SendAsync("ConfirmLoaded", gameId);
    }

    public ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
            return hubConnection.DisposeAsync();
        
        return ValueTask.CompletedTask;
    }
}