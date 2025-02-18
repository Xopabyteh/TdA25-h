using ErrorOr;
using h.Primitives;
using h.Primitives.Games;

namespace h.Server.Infrastructure.MultiplayerGames;

/// <summary>
/// Service which holds game sessions for multiplayer games.
/// Does not provide any game logic, only manages game sessions.
/// Does not notify players about game state changes.
/// </summary>
public interface IMultiplayerGameSessionService
{
    public Task<MultiplayerGameSession> CreateGameSessionAsync(IReadOnlyCollection<Guid> players);
    /// <summary>
    /// Confirm that the player has loaded the game.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if all players have been confirmed
    /// and game can be safely started with <see cref="StartGameAsync(Guid)"/>
    /// </returns>
    public Task<ErrorOr<bool>> ConfirmPlayerLoadedAsync(Guid gameId, Guid playerId);
    public Task<(Guid StartingPlayer, KeyValuePair<Guid, GameSymbol>[] PlayerSymbols)> StartGameAsync(Guid gameId);
    public Task<ErrorOr<Unit>> PlaceSymbolAsync(Guid gameId, Guid byPlayerId, Int2 atPos);

    public Task<MultiplayerGameSession> GetGameAsync(Guid byGameId);
}
