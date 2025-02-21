namespace h.Contracts.Leaderboard;
public readonly record struct LeaderBoardEntryResponse(
    string Username,
    int EloRating
);