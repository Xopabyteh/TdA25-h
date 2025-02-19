using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;
public readonly record struct MultiplayerGameStartedResponse(
    Guid GameId,
    MultiplayerGameUserIdentityDTO StartingPlayer,
    IReadOnlyCollection<MultiplayerGameUserIdentityDTO> Players,
    KeyValuePair<MultiplayerGameUserIdentityDTO, GameSymbol>[] PlayerSymbols
);
