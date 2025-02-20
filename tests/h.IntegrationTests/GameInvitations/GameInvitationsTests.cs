using h.Contracts.GameInvitations;
using Microsoft.AspNetCore.SignalR.Client;

namespace h.IntegrationTests.GameInvitations;
public class GameInvitationsTests
{
    [ClassDataSource<CustomWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public static CustomWebApplicationFactory _sessionApiFactory { get; set; } = null!;

    [Test]
    public async Task GameInvitations_GuestCreatesCode()
    {
        // Arange
        var (guestClient, guestAuth) = await _sessionApiFactory.LoginGuestAsync();
        await using var userConnection = _sessionApiFactory.CreateSignalRConnection(IGameInvitationHubClient.Route, guestAuth.Token);

        // Act
        await userConnection.StartAsync();

        var response = await guestClient.PostAsync("/api/v1/invitation/create", content: null);
        var inviteCode = await response.Content.ReadAsStringAsync();
        
        var didParse = int.TryParse(inviteCode, out var codeInt);

        // Assert
        response.EnsureSuccessStatusCode();
        await Assert.That(inviteCode).IsNotNull();
        await Assert.That(didParse).IsTrue();
        await Assert.That(inviteCode.Length).IsEqualTo(6);

        // Dispose
        guestClient.Dispose();
        await userConnection.StopAsync();
    }

    
    [Test]
    public async Task GameInvitations_PlayerNotConnecctedToHub_CannotMakeInvite()
    {
        // Arange
        var user = await _sessionApiFactory.LoginGuestAsync();
        
        // Act
        var response = await user.client.PostAsync("/api/v1/invitation/create", content: null);

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsFalse();

        // Dispose
        user.client.Dispose();
    }

    [Test]
    [Timeout(20_000)]
    public async Task GameInvitations_PlayerCreatesCode_AnotherJoins_AndGameStarts(CancellationToken cancellationToken)
    {
        var (client1, auth1) = await _sessionApiFactory.CreateUserAndLoginAsync(
            $"user1-{nameof(GameInvitations_PlayerCreatesCode_AnotherJoins_AndGameStarts)}",
            400
        );
        var (client2, auth2) = await _sessionApiFactory.LoginGuestAsync();
        await using var userConnection1 = _sessionApiFactory.CreateSignalRConnection(IGameInvitationHubClient.Route, auth1.Token);
        await using var userConnection2 = _sessionApiFactory.CreateSignalRConnection(IGameInvitationHubClient.Route, auth2.Token);

        var user1GameSessionCreated = new TaskCompletionSource<Guid>();
        var user2GameSessionCreated = new TaskCompletionSource<Guid>();

        userConnection1.On<Guid>(nameof(IGameInvitationHubClient.NewGameSessionCreated), user1GameSessionCreated.SetResult);
        userConnection2.On<Guid>(nameof(IGameInvitationHubClient.NewGameSessionCreated), user2GameSessionCreated.SetResult);

        // Act
        await userConnection1.StartAsync(cancellationToken);
        await userConnection2.StartAsync(cancellationToken);

        var createResponse = await client1.PostAsync("/api/v1/invitation/create", content: null, cancellationToken);
        var inviteCode = int.Parse(await createResponse.Content.ReadAsStringAsync());

        var joinResponse = await client2.PostAsync($"/api/v1/invitation/join/{inviteCode}", content: null, cancellationToken);

        // Wait for game session to be created (or timeout)
        await Task.WhenAll(user1GameSessionCreated.Task, user2GameSessionCreated.Task).WaitAsync(cancellationToken);

        // Assert
        await Assert.That(createResponse.IsSuccessStatusCode).IsTrue();
        await Assert.That(joinResponse.IsSuccessStatusCode).IsTrue();
        await Assert.That(user1GameSessionCreated.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(user2GameSessionCreated.Task.IsCompletedSuccessfully).IsTrue();
        await Assert.That(user1GameSessionCreated.Task.Result).IsEqualTo(user2GameSessionCreated.Task.Result);

        // Dispose
        client1.Dispose();
        client2.Dispose();
        await userConnection1.StopAsync();
    }
}
