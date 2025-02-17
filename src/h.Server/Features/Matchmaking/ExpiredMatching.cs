namespace h.Server.Features.Matchmaking;

public readonly record struct ExpiredMatching(
    Guid MatchingId,
    IReadOnlyCollection<Guid> HangingAcceptees
);
