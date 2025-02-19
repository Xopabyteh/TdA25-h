namespace h.Contracts.MultiplayerGames;
public interface IMultiplayerGameSessionHubClient
{
    public const string Route = "hub/multiplayer-game";

    public Task GameStarted(MultiplayerGameStartedResponse response);
    public Task PlayerMadeMove(PlayerMadeMoveResponse response);
    public Task GameEnded(MultiplayerGameEndedResponse response);
}
