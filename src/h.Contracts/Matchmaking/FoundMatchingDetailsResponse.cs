namespace h.Contracts.Matchmaking;
public readonly record struct FoundMatchingDetailsResponse(
    Guid MatchId,
    FoundMatchingPlayerDetailDto Player1,
    FoundMatchingPlayerDetailDto Player2
);

/// <summary>
/// Info about player to be displayed in a notification
/// about a found matching
/// </summary>
public readonly record struct FoundMatchingPlayerDetailDto(
    Guid PlayerId,
    string PlayerName,
    ulong EloRating
);


