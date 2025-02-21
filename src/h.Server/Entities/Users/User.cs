using h.Primitives.Users;
using h.Server.Entities.MultiplayerGames;

namespace h.Server.Entities.Users;

/// <summary>
/// Both an identity user and a user in the context of the application.
/// </summary>
public class User
{
    public Guid Uuid { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    
    public required string Username { get; set; }
    public required string Email { get; set; }

    public required UserRole[] Roles { get; set; }

    public ICollection<UserToFinishedRankedGame> UserToFinishedRankedGames { get; }
        = new List<UserToFinishedRankedGame>(); // Navigation property

    /// <summary>
    /// Refers to the password hash.
    /// </summary>
    public required string PasswordHash { get; set; }
    
    public ThinkDifferentElo Elo { get; set; }
    public int WinAmount { get; set; }
    public int LossAmount { get; set; }
    public int DrawAmount { get; set; }

    /// <summary>
    /// When <see langword="null"/>, the user is not banned.
    /// </summary>
    public DateTime? BannedFromRankedMatchmakingAt { get; set; }

    public static User NewUser(
        string username,
        string email,
        string passwordHash)
    {
        return new User()
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Elo = new()
            {
                Rating = ThinkDifferentElo.INITIAL_ELO
            },
            Roles = [],
        };
    }

     public static User NewUser(
        string username,
        string email,
        string passwordHash,
        int eloRating)
    {
        return new User()
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Elo = new()
            {
                Rating = eloRating
            },
            Roles = [],
        };
    }
}
