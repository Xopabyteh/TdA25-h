namespace h.Contracts.Users;

public readonly record struct AdminUpdateUserRequest(
    string Username,
    string Email,
    string? Password,
    int Elo,
    int WinAmount,
    int DrawAmount,
    int LossAmount
);