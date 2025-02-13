using h.Primitives.Users;

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

    /// <summary>
    /// Refers to the password hash.
    /// </summary>
    public required string PasswordHash { get; set; }
    
    public ThinkDifferentElo Elo { get; set; }
    public ulong WinAmount { get; set; }
    public ulong LossAmount { get; set; }
    public ulong DrawAmount { get; set; }

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
            Roles = []
        };
    }
}
