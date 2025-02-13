namespace h.Contracts.Users;
public readonly record struct RegisterUserRequest(
    string Username,
    string Email,
    string Password);
