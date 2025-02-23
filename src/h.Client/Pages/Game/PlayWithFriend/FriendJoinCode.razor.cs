using Blazored.SessionStorage;
using h.Client.Pages.Game.Multiplayer;
using h.Client.Services;
using h.Contracts.GameInvitations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game.PlayWithFriend;

public partial class FriendJoinCode : IAsyncDisposable
{
    [SupplyParameterFromQuery(Name = "c")] public int? RoomCodeQuery { get; set; }
    [Inject] protected NavigationManager _navigationManager { get; set; } = null!;
    [Inject] protected IHApiClient _api { get; set; } = null!;
    [Inject] protected ToastService _toastService { get; set; } = null!;
    [Inject] protected ISessionStorageService _sessionStorageService { get; set; } = null!;

    private bool isjoiningRoom;
    private HubConnection? hubConnection;
    private int? roomCode;

    protected override async Task OnInitializedAsync()
    {
        if(RoomCodeQuery is not null)
        {
            isjoiningRoom = true;
            roomCode = RoomCodeQuery;

            if(RuntimeInformation.ProcessArchitecture == Architecture.Wasm)
            {
                await HandleJoinRoom();
            }
        }
    }

    private async Task HandleJoinRoom()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        if(roomCode is null)
            return;

        isjoiningRoom = true;

        hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_navigationManager.BaseUri}{IGameInvitationHubClient.Route}")
            .Build();

        hubConnection.On<Guid>(nameof(IGameInvitationHubClient.NewGameSessionCreated), gameId =>
        {
            Console.WriteLine($"Match found {gameId}");
            _sessionStorageService.SetItemAsync(MultiplayerGame.GameIdSessionStorageKey, gameId);

            _navigationManager.NavigateTo(PageRoutes.Multiplayer.MultiplayerGame);
        });

        await hubConnection.StartAsync();

        var result = await _api.JoinInviteRoom(roomCode.Value);
        if(!result.IsSuccessful)
        {
            isjoiningRoom = false;
            await _toastService.ErrorAsync("Místnost nenalezena");
            return;
        }
    }
    
    public ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
            return hubConnection.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
