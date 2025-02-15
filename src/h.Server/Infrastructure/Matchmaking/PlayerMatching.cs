namespace h.Server.Infrastructure.Matchmaking;

public readonly record struct PlayerMatching(
    DateTimeOffset CreatedAt,
    Guid Id,
    Guid Player1Id,
    Guid Player2Id
)
{
    /// <summary>
    /// After how many seconds the matching is considered as timed out
    /// </summary>
    public const int WaitingMatchingTimeoutSeconds = 30;
}
