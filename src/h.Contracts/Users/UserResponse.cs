namespace h.Contracts.Users;

/// <summary>
/// User DTO
/// </summary>
/// <param name="Elo">Elo rating</param>
/// <param name="Wins">Amount of wins</param>
/// <param name="Draws">Amount of draws</param>
/// <param name="Losses">Amount of losses</param>
public readonly record struct UserResponse(
    Guid Uuid,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Username,
    string Email,
    int Elo,
    int Wins,
    int Draws,
    int Losses
);
