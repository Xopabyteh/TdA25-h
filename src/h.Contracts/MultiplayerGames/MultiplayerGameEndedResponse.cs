namespace h.Contracts.MultiplayerGames;
public readonly record struct MultiplayerGameEndedResponse(
    bool IsDraw,
    Guid? WinnerId
    //int EloChange
);
