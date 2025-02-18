namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSession
{
    public Guid Id { get; init; }
    public IReadOnlyCollection<Guid> Players { get; init; }
    public List<Guid> ReadyPlayers { get; init; } = new List<Guid>(2);
    
    public MultiplayerGameSession(Guid id, IReadOnlyCollection<Guid> players)
    {
        Id = id;
        Players = players;
    }
}
