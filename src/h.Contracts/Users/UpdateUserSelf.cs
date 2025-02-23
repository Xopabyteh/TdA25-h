namespace h.Contracts.Users;

public readonly record struct UpdateUserSelf(
    string Username,
    string Email,
    string? Password
);