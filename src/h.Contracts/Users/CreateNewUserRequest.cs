namespace h.Contracts.Users;
public readonly record struct CreateNewUserRequest(
    string Username,
    string Email,
    string Password,
    int Elo
);
