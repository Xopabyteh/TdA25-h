namespace h.Contracts.Users;

/// <summary>
/// Login and register response
/// </summary>
public readonly record struct AuthenticatiionResponse(
    string Token,
    UserResponse User
);