using Ardalis.SmartEnum;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace h.Primitives.Games;

public sealed class GameDifficulty : SmartEnum<GameDifficulty>
{
    public static readonly GameDifficulty Beginner = new GameDifficulty("beginner", (int)Enum.Beginner);
    public static readonly GameDifficulty Easy = new GameDifficulty("easy", (int)Enum.Easy);
    public static readonly GameDifficulty Medium = new GameDifficulty("medium", (int)Enum.Medium);
    public static readonly GameDifficulty Hard = new GameDifficulty("hard", (int)Enum.Hard);
    public static readonly GameDifficulty Extreme = new GameDifficulty("extreme", (int)Enum.Extreme);

    // Todo: refactor to use enum
    private GameDifficulty(string name, int value) : base(name, value) { }

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