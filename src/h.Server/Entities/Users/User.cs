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
    public required string PasswordEncrypted { get; set; }
    
    public ThinkDifferentElo Elo { get; set; }
    public int WinAmount { get; set; }
    public int LossAmount { get; set; }
    public int DrawAmount { get; set; }
}
