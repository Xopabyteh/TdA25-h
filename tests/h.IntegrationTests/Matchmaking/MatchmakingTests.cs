using h.Contracts.Matchmaking;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace h.IntegrationTests.Matchmaking;

public class MatchmakingTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public static CustomWebApplicationFactory _sessionApiFactory { get; set; } = null!;


    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified)]
    public async Task Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified()
    {
        // Arrange
        var (client1, _) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        var (client2, _) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        await using var hubConnection1 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        await using var hubConnection2 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var playerMatcherBGService = _sessionApiFactory.Services.GetRequiredService<MatchPlayersBackgroundService>();
        var scope = _sessionApiFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        FoundMatchingDetailsResponse? client1Matching = null;
        FoundMatchingDetailsResponse? client2Matching = null;
        var matchFoundTcs = new TaskCompletionSource();
        var lockObj = new object();

        hubConnection1.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), matching =>
        {
            lock (lockObj)
            {
                client1Matching = matching;
                if (client2Matching is not null)
                {
                    matchFoundTcs.TrySetResult();
                }
            }
        });

        hubConnection2.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), matching =>
        {
            lock(lockObj)
            {
                client2Matching = matching;
                if (client1Matching is not null)
                {
                    matchFoundTcs.TrySetResult();
                }
            }
        });

        // Act
        await hubConnection1.StartAsync();
        var response1 = await client1.PostAsync("/api/v1/matchmaking/join", content: null);
        response1.EnsureSuccessStatusCode();

        await hubConnection2.StartAsync();
        var response2 = await client2.PostAsync("/api/v1/matchmaking/join", content: null);
        response2.EnsureSuccessStatusCode();

        await playerMatcherBGService.MatchUsers(dbContext);

        // Wait for match to be found (or timeout)
        await Task.WhenAny(
            matchFoundTcs.Task,
            Task.Delay(TimeSpan.FromSeconds(10))
        );

        // Assert
        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.OK);

        await Assert.That(client1Matching).IsNotNull();
        await Assert.That(client2Matching).IsNotNull();

        await Assert.That(client1Matching!.Value.MatchId).IsEqualTo(client2Matching!.Value.MatchId);

        // Dispose
        await hubConnection1.StopAsync();
        await hubConnection2.StopAsync();

        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel)]
    public async Task Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel()
    {
// Arrange
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        var (client2, auth2) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        
        await using var hubConnection1 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        await using var hubConnection2 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var scope = _sessionApiFactory.Services.CreateScope();
        var playerMatcherBGService = _sessionApiFactory.Services.GetRequiredService<MatchPlayersBackgroundService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var matchmakingService = scope.ServiceProvider.GetRequiredService<InMemoryMatchmakingService>();

        var matchCancelledTcs = new TaskCompletionSource();
        var lockObj = new object();
        var client1MatchCancelNotified = false;
        var client2MatchCancelNotified = false;
        hubConnection1.On<Guid>(nameof(IMatchmakingHubClient.MatchCancelled), _ =>
        {
            lock (lockObj)
            {
                client1MatchCancelNotified = true;
                if (client2MatchCancelNotified)
                {
                    matchCancelledTcs.TrySetResult();
                }
            }
        });
        hubConnection2.On<Guid>(nameof(IMatchmakingHubClient.MatchCancelled), _ =>
        {
            lock (lockObj)
            {
                client2MatchCancelNotified = true;
                if (client1MatchCancelNotified)
                {
                    matchCancelledTcs.TrySetResult();
                }
            }
        });

        await hubConnection1.StartAsync();
        await hubConnection2.StartAsync();
        var matching = matchmakingService.RegisterNewPlayerMatching(auth1.User.Uuid, auth2.User.Uuid);
        await playerMatcherBGService.MatchUsers(dbContext);

// Act
        var declineResponse = await client1.PostAsync($"/api/v1/matchmaking/decline/{matching.Id}", content: null);

        // Wait for match to be cancelled (or timeout)
        await Task.WhenAny(
            matchCancelledTcs.Task,
            Task.Delay(TimeSpan.FromSeconds(10))
        );

// Assert
        // Notified
        await Assert.That(declineResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(client1MatchCancelNotified).IsTrue();
        await Assert.That(client2MatchCancelNotified).IsTrue();

        // Match doesn't exist
        await Assert.That(matchmakingService.GetMatching(matching.Id)).IsNull();

        // Dispose
        await hubConnection1.StopAsync();
        await hubConnection2.StopAsync();

        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue)]
    public async Task Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue()
    {
        // Arrange
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        var (client2, auth2) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        
        var scope = _sessionApiFactory.Services.CreateScope();
        var playerMatcherBGService = _sessionApiFactory.Services.GetRequiredService<MatchPlayersBackgroundService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var matchmakingService = scope.ServiceProvider.GetRequiredService<InMemoryMatchmakingService>();
        var queueService = scope.ServiceProvider.GetRequiredService<IMatchmakingQueueService>();

        var matching = matchmakingService.RegisterNewPlayerMatching(auth1.User.Uuid, auth2.User.Uuid);
        await playerMatcherBGService.MatchUsers(dbContext);

// Act
        var acceptResponse = await client1.PostAsync($"/api/v1/matchmaking/accept/{matching.Id}", content: null);
        var declineResponse = await client2.PostAsync($"/api/v1/matchmaking/decline/{matching.Id}", content: null);

// Assert
        // Match doesn't exist
        await Assert.That(matchmakingService.GetMatching(matching.Id)).IsNull();

        // Player 1 is back in queue
        await Assert.That(queueService.PeekFirstInQueue()).IsEqualTo(auth1.User.Uuid);

        // Dispose
        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_UserCannotJoinQueueTwice)]
    public async Task Matchmaking_UserCannotJoinQueueTwice()
    {
        // Arrange
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        // Act
        var response1 = await client1.PostAsync("/api/v1/matchmaking/join", content: null);
        var response2 = await client1.PostAsync("/api/v1/matchmaking/join", content: null);

        // Assert
        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    [NotInParallel(constraintKey: "expired-matchings", Order = MatchmakingTestOrder.Matchmaking_MatchingExpire_RemovesHangingMatchings_AndUsersGetNotified)]
    [MethodDataSource(typeof(FastMatchExpirationWebApplicationFactory.MethodDataSource), nameof(FastMatchExpirationWebApplicationFactory.MethodDataSource.Get))]
    public async Task Matchmaking_MatchingExpire_RemovesHangingMatchings_AndUsersGetNotified(
        FastMatchExpirationWebApplicationFactory _testApiFactory)
    {
        // Arrange
        var (client1, auth1) = await _testApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        var (client2, auth2) = await _testApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        await using var hubConnection1 = _testApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        await using var hubConnection2 = _testApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var scope = _testApiFactory.Services.CreateScope();
        var expiredMatchingsService = scope.ServiceProvider.GetRequiredService<RemoveExpiredMatchingsBackgroundService>();
        var matchmakingService = scope.ServiceProvider.GetRequiredService<InMemoryMatchmakingService>();
        
        var accepteeNotifiedAboutCancellationTcs = new TaskCompletionSource<bool>();
        var idlePlayerNotifiedAboutCancellationTcs = new TaskCompletionSource<bool>();

        hubConnection1.On<Guid>(nameof(IMatchmakingHubClient.MatchCancelled), _ =>
        {
            accepteeNotifiedAboutCancellationTcs.TrySetResult(true);
        });

        hubConnection2.On<Guid>(nameof(IMatchmakingHubClient.MatchCancelled), _ =>
        {
            idlePlayerNotifiedAboutCancellationTcs.TrySetResult(true);
        });

        await hubConnection1.StartAsync();
        await hubConnection2.StartAsync();

        // Act
        var matching = matchmakingService.RegisterNewPlayerMatching(auth1.User.Uuid, auth2.User.Uuid);
        
        await Task.Delay(TimeSpan.FromSeconds(FastMatchExpirationWebApplicationFactory.FastMatchExpirationSeconds+1));
        await expiredMatchingsService.RemoveExpiredMatchings();

        // Wait for both notifications about cancel (or timeout)
        await Task.WhenAny(
            Task.WhenAll(
                accepteeNotifiedAboutCancellationTcs.Task,
                idlePlayerNotifiedAboutCancellationTcs.Task
            ),

            Task.Delay(TimeSpan.FromSeconds(10))
                .ContinueWith(_ =>
                {
                    accepteeNotifiedAboutCancellationTcs.TrySetResult(false);
                    idlePlayerNotifiedAboutCancellationTcs.TrySetResult(false);
                })
        );
        
        // Assert
        await Assert.That(matchmakingService.GetMatching(matching.Id)).IsNull();
        await Assert.That(accepteeNotifiedAboutCancellationTcs.Task.Result).IsTrue();
        await Assert.That(idlePlayerNotifiedAboutCancellationTcs.Task.Result).IsTrue();

        // Dispose
        await hubConnection1.StopAsync();
        await hubConnection2.StopAsync();

        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_RemoveingHangingMatchings_PlacesAccepteesBackToQueue)]
    [MethodDataSource(typeof(FastMatchExpirationWebApplicationFactory.MethodDataSource), nameof(FastMatchExpirationWebApplicationFactory.MethodDataSource.Get))]
    public async Task Matchmaking_RemoveingHangingMatchings_PlacesAccepteesBackToQueue(
        FastMatchExpirationWebApplicationFactory _testApiFactory)
    {
        // Arrange
        var (client1, auth1) = await _testApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        var (client2, auth2) = await _testApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        await using var hubConnection1 = _testApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        await using var hubConnection2 = _testApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var scope = _testApiFactory.Services.CreateScope();
        var expiredMatchingsService = scope.ServiceProvider.GetRequiredService<RemoveExpiredMatchingsBackgroundService>();
        var matchmakingService = scope.ServiceProvider.GetRequiredService<InMemoryMatchmakingService>();
        var queueService = scope.ServiceProvider.GetRequiredService<IMatchmakingQueueService>();

        await hubConnection1.StartAsync();
        await hubConnection2.StartAsync();

        // Act
        var matching = matchmakingService.RegisterNewPlayerMatching(auth1.User.Uuid, auth2.User.Uuid);
        
        matchmakingService.AcceptMatching(matching.Id, auth1.User.Uuid);
        
        await Task.Delay(TimeSpan.FromSeconds(FastMatchExpirationWebApplicationFactory.FastMatchExpirationSeconds+1));
        await expiredMatchingsService.RemoveExpiredMatchings();

        var queuePeek = queueService.PeekQueue(tryTakeRange: 2).ToArray();

        // Assert
        await Assert.That(matchmakingService.GetMatching(matching.Id)).IsNull();
        await Assert.That(queueService.PeekFirstInQueue()).IsEqualTo(auth1.User.Uuid);
        await Assert.That(queuePeek).DoesNotContain(auth2.User.Uuid);

        // Dispose
        await hubConnection1.StopAsync();
        await hubConnection2.StopAsync();

        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    [NotInParallel(Order = MatchmakingTestOrder.Matchmaking_PlayersAccept_AndGetNotifiedAboutNewGameSession)]
    public async Task Matchmaking_PlayersAccept_AndGetNotifiedAboutNewGameSession()
    {
        // Arrange
        // Arrange
        var (client1, client1Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);
        var (client2, client2Auth) = await _sessionApiFactory.CreateUserAndLoginAsync(
            eloRating: 400);

        await using var hubConnection1 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        await using var hubConnection2 = _sessionApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var scope = _sessionApiFactory.Services.CreateScope();
        var playerMatchingService = scope.ServiceProvider.GetRequiredService<InMemoryMatchmakingService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client1NotifiedAboutGameSessionTcs = new TaskCompletionSource<Guid>();
        var client2NotifiedAboutGameSessionTcs = new TaskCompletionSource<Guid>();
        hubConnection1.On<Guid>(
            nameof(IMatchmakingHubClient.NewGameSessionCreated),
            gameId => client1NotifiedAboutGameSessionTcs.TrySetResult(gameId));
        hubConnection2.On<Guid>(
            nameof(IMatchmakingHubClient.NewGameSessionCreated),
            gameId => client2NotifiedAboutGameSessionTcs.TrySetResult(gameId));
        
        await hubConnection1.StartAsync();
        await hubConnection2.StartAsync();
        
        var matching = playerMatchingService.RegisterNewPlayerMatching(
            client1Auth.User.Uuid,
            client2Auth.User.Uuid
        );

        // Act
        // Accept match
        var acceptResponse1 = await client1.PostAsync($"/api/v1/matchmaking/accept/{matching.Id}", content: null);
        var acceptResponse2 = await client2.PostAsync($"/api/v1/matchmaking/accept/{matching.Id}", content: null);

        // Wait for notification about game session (or timeout)
        var delayTask2 = Task.Delay(TimeSpan.FromSeconds(10));
        var resultTask2 = await Task.WhenAny(
            Task.WhenAll(client1NotifiedAboutGameSessionTcs.Task, client2NotifiedAboutGameSessionTcs.Task),
            delayTask2
        );

        // Assert
        // Acceptance assertions
        await Assert.That(acceptResponse1.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(acceptResponse2.StatusCode).IsEqualTo(HttpStatusCode.OK);

        await Assert.That(client1NotifiedAboutGameSessionTcs.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(client2NotifiedAboutGameSessionTcs.Task.IsCompletedSuccessfully).IsTrue();

        await Assert.That(client1NotifiedAboutGameSessionTcs.Task.Result)
            .IsEqualTo(client2NotifiedAboutGameSessionTcs.Task.Result);

        // Dispose
        await hubConnection1.StopAsync();
        await hubConnection2.StopAsync();

        client1.Dispose();
        client2.Dispose();
    }

    [Test]
    public async Task Matchmaking_AdminCannotJoinMatchmaking()
    {
        // Arrange
        var (client, _) = await _sessionApiFactory.LoginAdminAsync();

        // Act
        var response = await client.PostAsync("/api/v1/matchmaking/join", content: null);
        
        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        // Dispose
        client.Dispose();
    }
}
