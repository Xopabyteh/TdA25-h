using h.Server.Entities.Users;

namespace h.Server.Entities.MultiplayerGames;

public class UserToFinishedRankedGame
{
    public Guid UserId { get; set; }
    public User? User { get; set; } // Navigation property

    public int FinishedRankedGameId { get; set; }
    public FinishedRankedGame? FinishedRankedGame { get; set; } // Navigation property
}
