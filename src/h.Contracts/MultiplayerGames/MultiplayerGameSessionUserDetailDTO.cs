using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

public readonly record struct MultiplayerGameSessionUserDetailDTO(
    MultiplayerGameUserIdentityDTO Identity,
    GameSymbol Symbol,
    string Name,
    int? EloRating,
    TimeSpan StartingTimeOnClock
);
