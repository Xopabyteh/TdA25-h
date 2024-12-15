using Ardalis.SmartEnum;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace h.Primitives.Games;

public sealed class GameDifficulty : SmartEnum<GameDifficulty>
{
    public static readonly GameDifficulty Beginner = new GameDifficulty("beginner", 0);
    public static readonly GameDifficulty Easy = new GameDifficulty("easy", 1);
    public static readonly GameDifficulty Medium = new GameDifficulty("medium", 2);
    public static readonly GameDifficulty Hard = new GameDifficulty("hard", 3);
    public static readonly GameDifficulty Extreme = new GameDifficulty("extreme", 4);

    private GameDifficulty(string name, int value) : base(name, value) { }
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