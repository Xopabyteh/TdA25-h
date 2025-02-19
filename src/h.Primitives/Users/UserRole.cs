namespace h.Primitives.Users;

/// <summary>
/// Shall be used with <see cref="nameof()"/> or <see cref="Enum.GetNames(Type)"/> 
/// when used in claims
/// </summary>
public enum UserRole
{
    Admin = 0b_0000_0001,
    Guest = 0b_000_0001 << 1,
}
