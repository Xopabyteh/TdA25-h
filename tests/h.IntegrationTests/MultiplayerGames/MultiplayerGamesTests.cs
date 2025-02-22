using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace h.IntegrationTests.MultiplayerGames;

[Retry(2)]
public class MultiplayerGamesTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public static CustomWebApplicationFactory _sessionApiFactory { get; set; } = null!;

    [Test]
    public async Task MultiplayerGames_PlayersConfirmLoad_AndGameStarts()
    {
// Arrange
        var (client1, client1Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        var (client2, client2Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        await using var client1Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client1Auth.Token);
        await using var client2Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client2Auth.Token);

        var scope = _sessionApiFactory.Services.CreateScope();
        var gameSessionService = scope.ServiceProvider.GetRequiredService<IMultiplayerGameSessionService>();
        var gameSession = gameSessionService.CreateGameSession([client1Auth.User.Uuid, client2Auth.User.Uuid]);

        var client1GameStartedTcs = new TaskCompletionSource<MultiplayerGameStartedResponse>();
        var client2GameStartedTcs = new TaskCompletionSource<MultiplayerGameStartedResponse>();

        client1Connection.On<MultiplayerGameStartedResponse>(nameof(IMultiplayerGameSessionHubClient.GameStarted), 
            client1GameStartedTcs.SetResult);
        client2Connection.On<MultiplayerGameStartedResponse>(nameof(IMultiplayerGameSessionHubClient.GameStarted),
            client2GameStartedTcs.SetResult);

        await client1Connection.StartAsync();
        await client2Connection.StartAsync();

// Act
        // Confirm load
        await client1Connection.InvokeAsync("ConfirmLoaded", gameSession.Id);
        await client2Connection.InvokeAsync("ConfirmLoaded", gameSession.Id);

        // Wait for game to start (or timeout)
        var delayTask = Task.Delay(TimeSpan.FromSeconds(10));
        var waitForLoadTask = await Task.WhenAny(
            Task.WhenAll(client1GameStartedTcs.Task, client2GameStartedTcs.Task),
            delayTask
        );
        
        if (waitForLoadTask == delayTask)
            Assert.Fail("Timed out, clients didn't get game started notification");

        // Assert
        await Assert.That(client1GameStartedTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(client2GameStartedTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(client1GameStartedTcs.Task.Result.GameId == gameSession.Id).IsTrue();
        await Assert.That(client2GameStartedTcs.Task.Result.GameId == gameSession.Id).IsTrue();

        // Session identity
        await Assert.That(client1GameStartedTcs.Task.Result.PlayerIdentities).Contains(p => p.SessionId == client1GameStartedTcs.Task.Result.MySessionId);
        await Assert.That(client1GameStartedTcs.Task.Result.PlayerIdentities).Contains(p => p.SessionId == client1GameStartedTcs.Task.Result.MySessionId);
        await Assert.That(client2GameStartedTcs.Task.Result.PlayerIdentities).Contains(p => p.SessionId == client2GameStartedTcs.Task.Result.MySessionId);
        await Assert.That(client2GameStartedTcs.Task.Result.PlayerIdentities).Contains(p => p.SessionId == client2GameStartedTcs.Task.Result.MySessionId);

        // Session identity matches fabricable identity (Prone to change)
        await Assert.That(client1GameStartedTcs.Task.Result.MySessionId).IsEqualTo(MultiplayerGameUserIdentity.FromUserId(client1Auth.User.Uuid).UserId!.Value);
        await Assert.That(client2GameStartedTcs.Task.Result.MySessionId).IsEqualTo(MultiplayerGameUserIdentity.FromUserId(client2Auth.User.Uuid).UserId!.Value);

        // Dispose
        scope.Dispose();
        client1.Dispose();
        client2.Dispose();
        await client1Connection.StopAsync();
        await client2Connection.StopAsync();
    }

    [Test]
    [Timeout(30_000)]
    public async Task MultiplayerGames_AuthedPlayersPlayGame_AndOneWins_AndStatisticsAreSaved(CancellationToken cancellationToken)
    {
        // Arrange
        var initialElo1 = 400;
        var initialElo2 = 400;

        var (client1, client1Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: initialElo1);
         var (client2, client2Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: initialElo2);
        await using var client1Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client1Auth.Token);
        await using var client2Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client2Auth.Token);
        
        await client1Connection.StartAsync(cancellationToken);
        await client2Connection.StartAsync(cancellationToken);

        var scope = _sessionApiFactory.Services.CreateScope();
        var gameSessionService = scope.ServiceProvider.GetRequiredService<IMultiplayerGameSessionService>();
        var gameSession = gameSessionService.CreateGameSession(
            [client1Auth.User.Uuid, client2Auth.User.Uuid],
            forcedStartingPlayerId: client1Auth.User.Uuid);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Fabricate identities, but they would be gathered from "GameStarted" event.
        var client1MultiplayerIdentity = MultiplayerGameUserIdentity.FromUserId(client1Auth.User.Uuid);
        var client2MultiplayerIdentity = MultiplayerGameUserIdentity.FromUserId(client2Auth.User.Uuid);

        gameSessionService.ConfirmPlayerLoaded(gameSession.Id, client1MultiplayerIdentity);
        gameSessionService.ConfirmPlayerLoaded(gameSession.Id, client2MultiplayerIdentity);

        var client1GameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();
        var client2GameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();

        // Act
        // Simulate players playing, client1 wins
        var client1X = 0;
        var client2X = 0;

        client1Connection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            if(response.NextPlayerOnTurn is null)
                return;

            // Place symbols in first row one after another
            var isOnTurn = response.NextPlayerOnTurn!.Value.SessionId == client1MultiplayerIdentity.SessionId;
            if(!isOnTurn)
                return;

            client1X++;
            await client1Connection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(client1X, 0));
        });

        client2Connection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            if(response.NextPlayerOnTurn is null)
                return;

            // Place symbols in second row with gaps (client2 can't win)
            var isOnTurn = response.NextPlayerOnTurn!.Value.SessionId == client2MultiplayerIdentity.SessionId;
            if (!isOnTurn)
                return;

            client2X += 2;
            await client2Connection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(client2X, 1));
        });

        client1Connection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded),
            client1GameEndedTcs.SetResult);
        client2Connection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded),
            client2GameEndedTcs.SetResult);

        var gameStartParams = gameSessionService.StartGame(gameSession.Id);

        // Client1 starts (forced)
        await client1Connection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(client1X, 0));

        // Wait for game to end
        await Task.WhenAll(client1GameEndedTcs.Task, client2GameEndedTcs.Task).WaitAsync(cancellationToken);
        
        // Assert statistics changed and we can find game in history
        var player1InDb = await db.UsersDbSet
            .Include(u => u.UserToFinishedRankedGames)
            .ThenInclude(ufg => ufg.FinishedRankedGame)
            .FirstAsync(u => u.Uuid == client1Auth.User.Uuid);
        var player2InDb = await db.UsersDbSet
            .Include(u => u.UserToFinishedRankedGames)
            .ThenInclude(ufg => ufg.FinishedRankedGame)
            .FirstAsync(u => u.Uuid == client2Auth.User.Uuid);

        // Assert
        await Assert.That(client1GameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(client2GameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();

        await Assert.That(client1GameEndedTcs.Task.Result.IsDraw).IsFalse();
        await Assert.That(client1GameEndedTcs.Task.Result.WinnerId!.Value.SessionId).IsEqualTo(client1MultiplayerIdentity.SessionId);

        await Assert.That(client2GameEndedTcs.Task.Result).IsEqualTo(client1GameEndedTcs.Task.Result);

        // Wins changed
        await Assert.That(player1InDb.WinAmount == 1).IsTrue();
        await Assert.That(player2InDb.LossAmount == 1).IsTrue();

        // Elo changed
        await Assert.That(player1InDb.Elo.Rating > initialElo1).IsTrue();
        await Assert.That(player2InDb.Elo.Rating < initialElo2).IsTrue();

        // Game history
        await Assert.That(player1InDb.UserToFinishedRankedGames).HasCount().EqualToOne();
        await Assert.That(player2InDb.UserToFinishedRankedGames).HasCount().EqualToOne();

        await Assert.That(player1InDb.UserToFinishedRankedGames.First()!.FinishedRankedGame!.Id)
            .IsEqualTo(player2InDb.UserToFinishedRankedGames.First()!.FinishedRankedGame!.Id);
    }

    [Test]
    [Timeout(30_000)]
    public async Task MultiplayerGames_GuestGameIsPlayed_AndStatisticsArentSaved(CancellationToken cancellationToken)
    {
        // Arrange
        var (clientUser, clientUserAuth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        var (clientGuest, clientGuestAuth) = await _sessionApiFactory.LoginGuestAsync();

        await using var clientUserConnection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, clientUserAuth.Token);
        await using var clientGuestConnection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, clientGuestAuth.Token);
        await clientUserConnection.StartAsync(cancellationToken);
        await clientGuestConnection.StartAsync(cancellationToken);

        var clientUserMultiplayerIdentity = MultiplayerGameUserIdentity.FromUserId(clientUserAuth.User.Uuid);
        var clientGuestMultiplayerIdentity = MultiplayerGameUserIdentity.FromGuest(clientGuestAuth.GuestId);

        var scope = _sessionApiFactory.Services.CreateScope();
        var gameSessionService = scope.ServiceProvider.GetRequiredService<IMultiplayerGameSessionService>();
        var gameSession = gameSessionService.CreateGameSession(
            [
                clientUserMultiplayerIdentity,
                clientGuestMultiplayerIdentity
            ],
            forcedStartingPlayerId: clientUserMultiplayerIdentity
        );

        var clientUserGameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();
        var clientGuestGameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();

        // Act
        // Simulate players playing, client1 wins
        var clientUserX = 0;
        var clientGuestX = 0;

        clientUserConnection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            if (response.NextPlayerOnTurn is null)
                return;

            // Place symbols in first row one after another
            var isOnTurn = response.NextPlayerOnTurn!.Value.SessionId == clientUserAuth.User.Uuid;
            if (!isOnTurn)
                return;

            clientUserX++;
            await clientUserConnection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(clientUserX, 0));
        });
        clientGuestConnection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            if (response.NextPlayerOnTurn is null)
                return;
           
            // Place symbols in second row with gaps (client2 can't win)
            var isOnTurn = response.NextPlayerOnTurn!.Value.SessionId == clientGuestAuth.GuestId;
            if (!isOnTurn)
                return;

            clientGuestX += 2;
            await clientGuestConnection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(clientGuestX, 1));
        });

        clientUserConnection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded),
            clientUserGameEndedTcs.SetResult);

        clientGuestConnection.On<MultiplayerGameEndedResponse>(nameof(IMultiplayerGameSessionHubClient.GameEnded),
            clientGuestGameEndedTcs.SetResult);

        var gameStartParams = gameSessionService.StartGame(gameSession.Id);
        
        // Client1 starts (forced)
        await clientUserConnection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(clientUserX, 0), cancellationToken);

        await Task.WhenAll(clientUserGameEndedTcs.Task, clientGuestGameEndedTcs.Task).WaitAsync(cancellationToken) ;

        var playerUserInDb = await scope.ServiceProvider.GetRequiredService<AppDbContext>().UsersDbSet
            .Include(u => u.UserToFinishedRankedGames)
            .ThenInclude(ufg => ufg.FinishedRankedGame)
            .FirstAsync(u => u.Uuid == clientUserAuth.User.Uuid);

        // Assert
        await Assert.That(clientUserGameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(clientGuestGameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();

        await Assert.That(clientUserGameEndedTcs.Task.Result.IsDraw).IsFalse();
        await Assert.That(clientUserGameEndedTcs.Task.Result.WinnerId!.Value.SessionId).IsEqualTo(clientUserMultiplayerIdentity.SessionId);

        await Assert.That(clientGuestGameEndedTcs.Task.Result).IsEqualTo(clientUserGameEndedTcs.Task.Result);

        // Assert that statistics haven't changed
        await Assert.That(playerUserInDb.WinAmount == 0).IsTrue();
        await Assert.That(playerUserInDb.LossAmount == 0).IsTrue();
        await Assert.That(playerUserInDb.UserToFinishedRankedGames).IsEmpty();

        // Dispose
        scope.Dispose();
        clientUser.Dispose();
        clientGuest.Dispose();
        await clientUserConnection.StopAsync();
        await clientGuestConnection.StopAsync();
    }    
}

