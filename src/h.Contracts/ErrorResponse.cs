using ErrorOr;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace h.Contracts;

/// <param name="Code">HTTP Error response code, yeah, whyever the **** this is here</param>
/// <param name="Message">General message</param>
public readonly record struct ErrorResponse(
    int Code,
    string Message,
    IReadOnlyCollection<Error>? Errors)
{
    /// <summary>
    /// Considering the <see cref="nameof(Type)"/> code provided by
    /// <see cref="SharedErrors"/>, used inside <see cref="Error.Code"/>
    /// </summary>
    public bool TryFindError(string code, out Error? error)
    {
        if(Errors is null)
        {
            error = default;
            return false;
        }

        var defaultValue = default(Error);
        error = Errors.FirstOrDefault(e => e.Code == code, defaultValue);

        return error != defaultValue;
    }
}


public class ErrorConverter : JsonConverter<Error>
{
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        int type = root.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.Number
            ? typeProp.GetInt32()
            : 0; // Default value

        string code = root.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String
            ? codeProp.GetString() ?? string.Empty
            : string.Empty;

        string description = root.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String
            ? descProp.GetString() ?? string.Empty
            : string.Empty;

        var metadata = root.TryGetProperty("metadata", out var metaProp) && metaProp.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(metaProp.GetRawText(), options)
            : new Dictionary<string, object>();

        return Error.Custom(type, code, description, metadata);
    }

    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("type", (int)value.Type);
        writer.WriteString("code", value.Code);
        writer.WriteString("description", value.Description);

        writer.WritePropertyName("metadata");
        JsonSerializer.Serialize(writer, value.Metadata, options);

        writer.WriteEndObject();
    }
}