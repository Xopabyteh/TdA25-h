using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace h.IntegrationTests.MultiplayerGames;
public class MultiplayerGamesTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public static CustomWebApplicationFactory _sessionApiFactory { get; set; } = null!;

    [Test]
    public async Task MultiplayerGames_PlayersConfirmLoad_AndGameStarts()
    {
// Arrange
        var (client1, client1Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player1-{nameof(MultiplayerGames_PlayersConfirmLoad_AndGameStarts)}",
            eloRating: 400);
        var (client2, client2Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player2-{nameof(MultiplayerGames_PlayersConfirmLoad_AndGameStarts)}",
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

        // Dispose
        scope.Dispose();
        client1.Dispose();
        client2.Dispose();
        await client1Connection.StopAsync();
        await client2Connection.StopAsync();
    }

    [Test]
    [Timeout(30_000)]
    public async Task MultiplayerGames_PlayersPlayGame_AndOneWins(CancellationToken cancellationToken)
    {
        // Arrange
        var (client1, client1Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player1-{nameof(MultiplayerGames_PlayersConfirmLoad_AndGameStarts)}",
            eloRating: 400);
        var (client2, client2Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player2-{nameof(MultiplayerGames_PlayersConfirmLoad_AndGameStarts)}",
            eloRating: 400);
        await using var client1Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client1Auth.Token);
        await using var client2Connection = _sessionApiFactory.CreateSignalRConnection(IMultiplayerGameSessionHubClient.Route, client2Auth.Token);
        
        await client1Connection.StartAsync(cancellationToken);
        await client2Connection.StartAsync(cancellationToken);

        var scope = _sessionApiFactory.Services.CreateScope();
        var gameSessionService = scope.ServiceProvider.GetRequiredService<IMultiplayerGameSessionService>();
        var gameSession = gameSessionService.CreateGameSession(
            [client1Auth.User.Uuid, client2Auth.User.Uuid],
            forcedStartingPlayer: client1Auth.User.Uuid);

        gameSessionService.ConfirmPlayerLoaded(gameSession.Id, client1Auth.User.Uuid);
        gameSessionService.ConfirmPlayerLoaded(gameSession.Id, client2Auth.User.Uuid);

        var client1GameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();
        var client2GameEndedTcs = new TaskCompletionSource<MultiplayerGameEndedResponse>();

        // Act
        // Simulate players playing, client1 wins
        var client1X = 0;
        var client2X = 0;


        client1Connection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            // Place symbols in first row one after another
            var isOnTurn = response.NextPlayerOnTurn == client1Auth.User.Uuid;
            if(!isOnTurn)
                return;

            client1X++;
            await client1Connection.InvokeAsync("PlaceSymbol", gameSession.Id, new Int2(client1X, 0));
        });

        client2Connection.On<PlayerMadeMoveResponse>(nameof(IMultiplayerGameSessionHubClient.PlayerMadeMove), async response =>
        {
            // Place symbols in second row with gaps (client2 can't win)
            var isOnTurn = response.NextPlayerOnTurn == client2Auth.User.Uuid;
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

        // Assert
        await Assert.That(client1GameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(client2GameEndedTcs.Task.IsCompletedSuccessfully).IsTrue();

        await Assert.That(client1GameEndedTcs.Task.Result.IsDraw).IsFalse();
        await Assert.That(client1GameEndedTcs.Task.Result.WinnerId).IsEqualTo(client1Auth.User.Uuid);

        await Assert.That(client2GameEndedTcs.Task.Result).IsEqualTo(client1GameEndedTcs.Task.Result);
    }
}

