namespace h.Server.Entities.Users;

public struct ThinkDifferentElo
{
    public const ulong INITIAL_ELO = 400;

    /// <summary>
    /// Elo should be rounded up
    /// </summary>
    public ulong Rating { get; set; }
}
