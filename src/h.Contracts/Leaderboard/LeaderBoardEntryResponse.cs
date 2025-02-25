namespace h.Contracts.Leaderboard;
public readonly record struct LeaderBoardEntryResponse(
    string Username,
    int EloRating,
    int WinAmount,
    int LossAmount,
    int Rank
);