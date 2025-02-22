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
        error = Errors?.FirstOrDefault(e => e.Code == code);
        return error is not null;
    }

    //[JsonConstructor]
    //public ErrorResponse(int code, string message, List<Error>? errors)
    //    : this(code, message, errors?.AsReadOnly()) { }
}

public class ErrorConverter : JsonConverter<Error>
{
    public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        return Error.Custom(
            root.GetProperty("type").GetInt32(),
            root.GetProperty("code").GetString()!,
            root.GetProperty("description").GetString()!,
            root.GetProperty("metadata").Deserialize<Dictionary<string, object>>(options)
        );
    }

    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            code = value.Code,
            description = value.Description,
            type = value.Type,
            metadata = value.Metadata
        }, options);
    }
}