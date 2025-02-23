using h.Contracts;
using h.Primitives.Games;
using System.Text.Json;

namespace h.Client.Services;

/// <summary>
/// Has same converters as server to keep up with fucking TdA API spec
/// which forces some weird shit serialization
/// </summary>
public static class AppJsonOptions
{
    public static JsonSerializerOptions WithConverters { get; private set; }

    static AppJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };

        options.Converters.Add(new GameDifficultyJsonConverter());
        options.Converters.Add(new GameStateJsonConverter());
        options.Converters.Add(new ErrorConverter());

        WithConverters = options;
    }
}
