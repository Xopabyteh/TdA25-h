namespace h.Server.Features.LeaderBoard;

/// <summary>
/// </summary>
/// <param name="Rank">1 indexed</param>
public record LeaderBoardEntryView(
    string Username,
    int Rating,
    int WinAmount,
    int LossAmount,
    int Rank
);
