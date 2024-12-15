using h.Contracts;

namespace h.Server.Infrastructure;

public static class ErrorResults
{
    /// <summary>
    /// Obsah požadavku neodpovídá specifikaci - například chybí nějaké povinné pole.
    /// </summary>
    public static IResult BadRequest(string reason)
        => Results.BadRequest(new ErrorResponse(400, $"Bad request: {reason}"));

    /// <summary>
    /// Daný zdroj nebyl nalezen.
    /// </summary>
    public static IResult NotFound()
        => Results.NotFound(new ErrorResponse(404, "Resource not found"));

    /// <summary>
    /// Server rozumí požadavku (pole jsou správná), ale požadavek obsahuje
    /// sémantickou chybu - například je rozměr hry 3x3 a obsahuje znak '@'
    /// </summary>
    public static IResult ValidationError(string reason)
        => Results.UnprocessableEntity(new ErrorResponse(422, $"Semantic error: {reason}"));
}
