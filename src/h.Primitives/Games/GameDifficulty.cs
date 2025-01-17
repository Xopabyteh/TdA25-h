using Ardalis.SmartEnum;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace h.Primitives.Games;

public sealed class GameDifficulty : SmartEnum<GameDifficulty>
{
    public static readonly GameDifficulty Beginner = new GameDifficulty("beginner", Enum.Beginner);
    public static readonly GameDifficulty Easy = new GameDifficulty("easy", Enum.Easy);
    public static readonly GameDifficulty Medium = new GameDifficulty("medium", Enum.Medium);
    public static readonly GameDifficulty Hard = new GameDifficulty("hard", Enum.Hard);
    public static readonly GameDifficulty Extreme = new GameDifficulty("extreme", Enum.Extreme);
    
    private GameDifficulty(string name, Enum value)
        : base(name, (int)value) { }

    public Enum EnumValue
        => (Enum)Value;

    public enum Enum
    {
        Beginner = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Extreme = 4
    }
}
public class GameDifficultyJsonConverter : JsonConverter<GameDifficulty>
{
    public override GameDifficulty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return GameDifficulty.TryFromName(value, out var result) ? result : throw new JsonException($"Unable to parse '{value}' to GameDifficulty.");
    }

    public override void Write(Utf8JsonWriter writer, GameDifficulty value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}