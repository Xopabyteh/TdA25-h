using h.Server.Infrastructure.Matchmaking;

namespace h.Server.Features.Matchmaking;

public readonly record struct ExpiredMatching(
    PendingPlayerMatching Matching,
    IReadOnlyCollection<Guid> HangingAcceptees
);
