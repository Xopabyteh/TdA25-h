﻿using ErrorOr;
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
    /// <summary>
    /// Create a game session with the given players.
    /// They may be guests or registered users.
    /// </summary>
    public MultiplayerGameSession CreateGameSession(
        IReadOnlyList<MultiplayerGameUserIdentity> players,
        MultiplayerGameUserIdentity? forcedStartingPlayerId = null);

    /// <summary>
    /// Create a game session with the given players.
    /// They are all registered users with their given userIDs.
    /// </summary>
    public MultiplayerGameSession CreateGameSession(
        IReadOnlyList<Guid> playerIds,
        Guid? forcedStartingPlayerId = null);

    /// <summary>
    /// Confirm that the player has loaded the game.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if all players have been confirmed
    /// and game can be safely started with <see cref="StartGame(Guid)"/>
    /// </returns>
    public ErrorOr<bool> ConfirmPlayerLoaded(Guid gameId, MultiplayerGameUserIdentity playerId);
    public (MultiplayerGameUserIdentity StartingPlayer, KeyValuePair<MultiplayerGameUserIdentity, GameSymbol>[] PlayerSymbols) StartGame(Guid gameId);

    /// <summary>
    /// Places a symbol and moves the turn to the next player.
    /// If the new symbol results in a win (or draw), the game is ended
    /// and the results can be obtained via <see cref="GetEndResult(Guid)"/>.
    /// </summary>
    /// <returns>
    /// The new player on turn ID.
    /// <see langword="null"/> if the game is over - no "next" player to play.
    /// </returns>
    public ErrorOr<MultiplayerGameUserIdentity?> PlaceSymbolAsyncAndMoveTurn(Guid gameId, MultiplayerGameUserIdentity byPlayer, Int2 atPos);

    public MultiplayerGameSessionEndResult? GetEndResult(Guid gameId);

    public MultiplayerGameSession? GetGame(Guid byGameId);
}
