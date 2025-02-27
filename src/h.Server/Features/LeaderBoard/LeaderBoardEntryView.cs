namespace h.Server.Features.LeaderBoard;

/// <summary>
/// </summary>
/// <param name="Rank">1 indexed</param>
public record LeaderBoardEntryView(
    string Username,
    Guid Uuid,
    int Rating,
    int WinAmount,
    int LossAmount,
    int DrawAmount,
    int Rank
);
