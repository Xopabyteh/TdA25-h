using h.Contracts.Matchmaking;
using h.IntegrationTests.Auth;
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
    [NotInParallel(Order = 10)]
    public async Task Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified()
    {
        // Arrange
        var (client1, _) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player1-{nameof(Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified)}",
            eloRating: 400);
        var (client2, _) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player2-{nameof(Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified)}",
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
    [NotInParallel(Order = 20)]
    public async Task Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel_AndMatchDoesntExist()
    {
// Arrange
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player1-{nameof(Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel_AndMatchDoesntExist)}",
            eloRating: 400);
        var (client2, auth2) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player2-{nameof(Matchmaking_UserDeclinesMatch_AndPlayersGetNotifiedAboutCancel_AndMatchDoesntExist)}",
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
    [NotInParallel(Order = 30)]
    public async Task Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue()
    {
        // Arrange
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player1-{nameof(Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue)}",
            eloRating: 400);
        var (client2, auth2) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"player2-{nameof(Matchmaking_UserDeclinesMath_AndOtherPlayerIsPlacedBackToQueue)}",
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
        await Assert.That(queueService.GetFirstInQueue()).IsEqualTo(auth1.User.Uuid);

        // Dispose
        client1.Dispose();
        client2.Dispose();
    }
}
