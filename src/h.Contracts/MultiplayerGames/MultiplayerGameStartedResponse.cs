using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;
public readonly record struct MultiplayerGameStartedResponse(
    Guid GameId,
    KeyValuePair<Guid, GameSymbol>[] PlayerSymbols,
    Guid StartingPlayer
);
