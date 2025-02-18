namespace h.Contracts.MultiplayerGames;
public interface IMultiplayerGameSessionHubClient
{
    public const string Route = "multiplayer-game";

    public Task GameStarted(GameStartedResponse response);
}
