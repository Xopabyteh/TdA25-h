namespace h.Contracts.MultiplayerGames;
/// <summary>
/// Received per client
/// </summary>
/// <param name="IsDraw"></param>
/// <param name="WinnerId"></param>
/// <param name="OldElo"></param>
/// <param name="NewElo"></param>
public readonly record struct MultiplayerGameEndedResponse(
    bool IsDraw,
    MultiplayerGameUserIdentityDTO? WinnerId,
    bool DidEloChange,
    int OldElo,
    int NewElo
);
