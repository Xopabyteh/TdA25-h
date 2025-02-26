namespace h.Contracts.Users;

public readonly record struct UpdateUserRequestTdASpec(
    string Username,
    string Email,
    string? Password,
    int Elo
);
