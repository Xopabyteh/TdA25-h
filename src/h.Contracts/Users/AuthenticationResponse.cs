namespace h.Contracts.Users;

/// <summary>
/// Login and register response
/// </summary>
public readonly record struct AuthenticationResponse(
    string Token,
    UserResponse User
);