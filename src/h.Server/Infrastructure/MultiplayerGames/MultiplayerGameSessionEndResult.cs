namespace h.Server.Infrastructure.MultiplayerGames;

public record MultiplayerGameSessionEndResult(
    bool IsDraw,
    MultiplayerGameUserIdentity? WinnerId
);