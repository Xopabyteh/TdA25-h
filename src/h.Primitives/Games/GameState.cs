using Ardalis.SmartEnum;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace h.Primitives.Games;

/// <summary>
/// In which phase the game is currently in
/// </summary>
public class GameState : SmartEnum<GameState>
{
    public static readonly GameState Opening = new GameState("opening", 0);
    public static readonly GameState Midgame = new GameState("midgame", 1);
    public static readonly GameState Endgame = new GameState("endgame", 2);
    public static readonly GameState Unknown = new GameState("unknown", 3);

    public GameState(string name, int value) : base(name, value)
    {
    }
}
public class GameStateJsonConverter : JsonConverter<GameState>
{
    public override GameState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return GameState.TryFromName(value, out var result) ? result : throw new JsonException($"Unable to parse '{value}' to GameState.");
    }

    public override void Write(Utf8JsonWriter writer, GameState value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
