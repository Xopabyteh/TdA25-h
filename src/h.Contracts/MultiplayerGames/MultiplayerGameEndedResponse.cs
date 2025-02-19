namespace h.Contracts.MultiplayerGames;
public readonly record struct MultiplayerGameEndedResponse(
    bool IsDraw,
    MultiplayerGameUserIdentityDTO? WinnerId
    //int EloChange
);
