using Ardalis.SmartEnum;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace h.Server.Entities.Games;

public sealed class GameDifficulty : SmartEnum<GameDifficulty>, IParsable<GameDifficulty>
{
    public static readonly GameDifficulty Beginner = new GameDifficulty("beginner", 0);
    public static readonly GameDifficulty Easy = new GameDifficulty("easy", 1);
    public static readonly GameDifficulty Medium = new GameDifficulty("medium", 2);
    public static readonly GameDifficulty Hard = new GameDifficulty("hard", 3);
    public static readonly GameDifficulty Extreme = new GameDifficulty("extreme", 4);

    private GameDifficulty(string name, int value) : base(name, value) { }

    /// <summary>
    /// Custom ASP.NET binding
    /// </summary>
    public static GameDifficulty Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result) ? result! : throw new FormatException();   
    }

    /// <summary>
    /// Custom ASP.NET binding
    /// </summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out GameDifficulty result)
    {
        result = s switch
        {
            "beginner" => Beginner,
            "easy" => Easy,
            "medium" => Medium,
            "hard" => Hard,
            "extreme" => Extreme,
            _ => null
        };

        return result != null;
    }
}
public class GameDifficultyJsonConverter : JsonConverter<GameDifficulty>
{
    public override GameDifficulty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return GameDifficulty.TryParse(value, null, out var result) ? result : throw new JsonException($"Unable to parse '{value}' to GameDifficulty.");
    }

    public override void Write(Utf8JsonWriter writer, GameDifficulty value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}