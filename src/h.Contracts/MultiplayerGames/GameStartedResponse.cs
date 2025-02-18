using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;
public readonly record struct GameStartedResponse(
    Guid gameId,
    KeyValuePair<Guid, GameSymbol>[] playerSymbols,
    Guid startingPlayer
);
