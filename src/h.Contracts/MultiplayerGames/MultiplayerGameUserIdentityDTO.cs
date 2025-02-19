namespace h.Contracts.MultiplayerGames;

public readonly record struct MultiplayerGameUserIdentityDTO(
    Guid SessionId,
    bool IsGuest
);