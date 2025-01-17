using ErrorOr;

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
}