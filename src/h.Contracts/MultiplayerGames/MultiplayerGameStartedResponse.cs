﻿using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

/// <summary>
/// A notification about a new game start. Each user recieves unique notification
/// with their <see cref="MySessionId"/>
/// </summary>
/// <param name="GameId">The id of the game</param>
/// <param name="MySessionId">The users multiplayer identity for this game session</param>
/// <param name="StartingPlayer">Starting player</param>
/// <param name="PlayerIdentities">Multiplayer identities of all players ingame</param>
/// <param name="PlayerSymbols">Player symbol mappings</param>
public readonly record struct MultiplayerGameStartedResponse(
    Guid GameId,
    Guid MySessionId,
    MultiplayerGameUserIdentityDTO StartingPlayer,
    IList<MultiplayerGameStartedUserDetail> Players
);

public readonly record struct MultiplayerGameStartedUserDetail(
    MultiplayerGameUserIdentityDTO Identity,
    GameSymbol Symbol,
    string Name,
    int? EloRating
);
