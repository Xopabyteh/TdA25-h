namespace h.Contracts.Matchmaking;

/// <summary>
/// Details about cancelled match
/// </summary>
/// <param name="NewPositionInQueue">When null, you were removed from the queue</param>
public readonly record struct MatchCancelledResponse(
    Guid MatchId,
    int? NewPositionInQueue
);
