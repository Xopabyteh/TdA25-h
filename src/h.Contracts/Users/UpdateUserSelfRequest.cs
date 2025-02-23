namespace h.Contracts.Users;

public readonly record struct UpdateUserSelfRequest(
    string Username,
    string Email,
    string? Password
);