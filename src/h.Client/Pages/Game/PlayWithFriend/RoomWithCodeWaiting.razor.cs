using Blazored.SessionStorage;
using h.Client.Pages.Game.Multiplayer;
using h.Client.Services;
using h.Contracts.GameInvitations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game.PlayWithFriend;

public partial class RoomWithCodeWaiting : IAsyncDisposable
{
    [Inject] protected IHApiClient _api { get; set; } = null!;
    [Inject] protected NavigationManager _navigationManager { get; set; } = null!;
    [Inject] protected ISessionStorageService _sessionStorage { get; set; } = null!;


    private HubConnection? hubConnection;
    private bool isLoaded;
    private int inviteCode;
    override protected async Task OnInitializedAsync()
    {
        // Wait for wasm,
        // Join hub,
        // Create code

        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_navigationManager.BaseUri}{IGameInvitationHubClient.Route}")
            .Build();

        hubConnection.On<Guid>(nameof(IGameInvitationHubClient.NewGameSessionCreated), gameId =>
        {
            _sessionStorage.SetItemAsync(MultiplayerGame.GameIdSessionStorageKey, gameId);

            _navigationManager.NavigateTo(PageRoutes.Multiplayer.MultiplayerGame);
        });

        await hubConnection.StartAsync();

        inviteCode = await _api.CreateInviteCode();

        isLoaded = true;
    }

    public ValueTask DisposeAsync()
    {
        if(hubConnection is not null)
            return hubConnection.DisposeAsync();

        return ValueTask.CompletedTask;
    }

    private string GetInviteLink()
        => $"{_navigationManager.BaseUri.AsSpan(0, _navigationManager.BaseUri.Length - 1)}{PageRoutes.Multiplayer.FriendJoinCodeWithQuery(inviteCode)}";
}
