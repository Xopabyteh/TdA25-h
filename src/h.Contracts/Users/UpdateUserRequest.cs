namespace h.Contracts.Users;

public readonly record struct UpdateUserRequest(
    string Username,
    string Email,
    string Password,
    int Elo
);