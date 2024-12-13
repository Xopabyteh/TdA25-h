using Ardalis.SmartEnum;
using System.Diagnostics.CodeAnalysis;

namespace h.Server.Entities.Games;

/// <summary>
/// In which phase the game is currently in
/// </summary>
public class GameState : SmartEnum<GameState>, IParsable<GameState>
{
    public static readonly GameState Opening = new GameState("opening", 0);
    public static readonly GameState Midgame = new GameState("midgame", 1);
    public static readonly GameState Endgame = new GameState("endgame", 2);
    public static readonly GameState Unknown = new GameState("unknown", 3);

    public GameState(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Custom ASP.NET binding
    /// </summary>
    /// <exception cref="FormatException"></exception>
    public static GameState Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result! : throw new FormatException();
    }

    /// <summary>
    /// Custom ASP.NET binding
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out GameState result)
    {
        result = s switch
        {
            "opening" => Opening,
            "midgame" => Midgame,
            "endgame" => Endgame,
            "unknown" => Unknown,
            _ => null
        };

        return result != null;
    }
}
