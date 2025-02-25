namespace h.Contracts.Leaderboard;

public readonly record struct LeaderBoardResponse(
    LeaderBoardEntryResponse[] PaginatedEntries,
    int TotalCount
    );
