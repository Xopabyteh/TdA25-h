namespace h.Contracts.Leaderboard;
public readonly record struct LeaderBoardEntryResponse(
    string Username,
    Guid Uuid,
    int EloRating,
    int WinAmount,
    int LossAmount,
    int DrawAmount,
    int Rank
);