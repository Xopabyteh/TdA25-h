using h.Primitives.Games;
using h.Server.Entities.Games;

namespace h.Server.Entities.MultiplayerGames;

/// <summary>
/// Represents a finished ranked game between 2 players.
/// </summary>
public class FinishedRankedGame
{
    public int Id { get; set; }
    public required GameBoard LastBoardState { get; init; }
    public required DateTime PlayedAt { get; init; }
    public required Guid Player1Id { get; init; }
    public required Guid Player2Id { get; init; }
    public required GameSymbol Player1Symbol { get; init; }
    public required GameSymbol Player2Symbol { get; init; }
    public required TimeSpan Player1RemainingTimer { get; init; }
    public required TimeSpan Player2RemainingTimer { get; init; }

    /// <summary>
    /// When <see langword="null"/> <see cref="IsDraw"/> shall be true.
    /// </summary>
    public required Guid? WinnerId { get; init; }
    public required bool IsDraw { get; init; }

    public ICollection<UserToFinishedRankedGame> UserToFinishedRankedGames { get; }
        = new List<UserToFinishedRankedGame>(); // Many-to-many mapping

    internal Guid GetOpponentUserId(Guid userId)
        => Player1Id == userId 
            ? Player2Id 
            : Player1Id;

    internal GameSymbol GetPlayerSymbol(Guid userId)
        => Player1Id == userId
            ? Player1Symbol
            : Player2Symbol;

    internal bool DidWin(Guid userId)
        => WinnerId == userId;
}
