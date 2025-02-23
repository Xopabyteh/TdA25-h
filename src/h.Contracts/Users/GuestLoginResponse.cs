namespace h.Contracts.Users;
public readonly record struct GuestLoginResponse(
    string Token,
    Guid GuestId,
    string Username
);