namespace h.Server.Entities.Users;

public class User
{
    public Guid Uuid { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordEncrypted { get; private set; }
    
    public ThinkDifferentElo Elo { get; set; }
    public int WinAmount { get; set; }
    public int LossAmount { get; set; }
    public int DrawAmount { get; set; }
}
