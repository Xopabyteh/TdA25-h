namespace h.Server.Infrastructure.Matchmaking;

public readonly record struct PendingPlayerMatching(
    DateTimeOffset CreatedAt,
    DateTimeOffset UnableAcceptAt,
    Guid Id,
    Guid Player1Id,
    Guid Player2Id
)
{
    public Guid[] GetPlayersInMatch()
    {
        return [Player1Id, Player2Id];
    }
}
