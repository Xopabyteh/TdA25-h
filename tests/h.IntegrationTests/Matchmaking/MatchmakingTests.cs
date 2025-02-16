using h.Contracts.Matchmaking;
using h.Server.Features.Matchmaking;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace h.IntegrationTests.Matchmaking;

public class MatchmakingTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerClass)]
    public static CustomWebApplicationFactory _classApiFactory { get; set; } = null!;
    
    [Test]
    [DependsOn(typeof(AuthTests), nameof(AuthTests.Login_ValidUser_ReturnsSuccess))]
    public async Task Matchmaking_UsersJoinMatch_GetMatched_AndGetNotified()
    {
        // Arrange
        using var client1 = await _classApiFactory.CreateUserAndLoginAsync(
            "player1",
            eloRating: 400);
        using var client2 = await _classApiFactory.CreateUserAndLoginAsync(
            "player2",
            eloRating: 400);

        var hubConnection1 = await _classApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client1.DefaultRequestHeaders.Authorization!.Parameter);

        var hubConnection2 = await _classApiFactory.CreateSignalRConnection(
            IMatchmakingHubClient.Route,
            client2.DefaultRequestHeaders.Authorization!.Parameter);

        var matchmakingBackgroundService = _classApiFactory.Services.GetRequiredService<MatchPlayersBackgroundService>();
        var scope = _classApiFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        FoundMatchingDetailsResponse? client1Matching = null;
        FoundMatchingDetailsResponse? client2Matching = null;
        var matchFoundTcs = new TaskCompletionSource();
        var matchFoundTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        hubConnection1.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), matching =>
        {
            client1Matching = matching;
            if(client2Matching is not null)
            {
                matchFoundTcs.SetResult();
            }
        });

        hubConnection2.On<FoundMatchingDetailsResponse>(nameof(IMatchmakingHubClient.MatchFound), matching =>
        {
            client2Matching = matching;
            if (client1Matching is not null)
            {
                matchFoundTcs.SetResult();
            }
        });

        // Act
        await hubConnection1.StartAsync();
        var response1 = await client1.PostAsync("/api/v1/matchmaking/join", content: null);
        
        await hubConnection2.StartAsync();
        var response2 = await client2.PostAsync("/api/v1/matchmaking/join", content: null);

        await matchmakingBackgroundService.MatchUsers(dbContext);

        // Wait for match to be found (or timeout)
        await Task.WhenAny(matchFoundTcs.Task, Task.Delay(-1, matchFoundTimeoutCts.Token));

        // Assert
        await Assert.That(response1.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response2.StatusCode).IsEqualTo(HttpStatusCode.OK);

        await Assert.That(client1Matching).IsNotNull();
        await Assert.That(client2Matching).IsNotNull();

        await Assert.That(client1Matching!.Value.MatchId).IsEqualTo(client2Matching!.Value.MatchId);
    }
}
