﻿using Carter.ModelBinding;
using ErrorOr;
using FluentValidation.Results;
using h.Contracts;

namespace h.Server.Infrastructure;

public static class ErrorResults
{
    /// <summary>
    /// Obsah požadavku neodpovídá specifikaci - například chybí nějaké povinné pole.
    /// </summary>
    public static IResult BadRequest(string reason, IReadOnlyCollection<Error>? Errors = null)
        => Results.BadRequest(new ErrorResponse(400, $"Bad request: {reason}", Errors));

    /// <summary>
    /// Obsah požadavku neodpovídá specifikaci - například chybí nějaké povinné pole.
    /// Default reason
    /// </summary>
    public static IResult BadRequest(IReadOnlyCollection<Error>? Errors = null)
        => Results.BadRequest(new ErrorResponse(400, $"Bad request: The content doesn't align with the specification. Ensure no fields are missing.", Errors));


    /// <summary>
    /// Daný zdroj nebyl nalezen.
    /// </summary>
    public static IResult NotFound(IReadOnlyCollection<Error>? Errors = null)
        => Results.NotFound(new ErrorResponse(404, "Resource not found", Errors));

    /// <summary>
    /// Server rozumí požadavku (pole jsou správná), ale požadavek obsahuje
    /// sémantickou chybu - například je rozměr hry 3x3 a obsahuje znak '@'
    /// </summary>
    public static IResult ValidationError(string reason, IReadOnlyCollection<Error>? Errors = null)
        => Results.UnprocessableEntity(new ErrorResponse(422, $"Semantic error: {reason}", Errors));

    /// <summary>
    /// Server rozumí požadavku (pole jsou správná), ale požadavek obsahuje
    /// sémantickou chybu - například je rozměr hry 3x3 a obsahuje znak '@'
    /// </summary>
    public static IResult ValidationError(IReadOnlyCollection<Error>? errors = null)
        => Results.UnprocessableEntity(new ErrorResponse(
            422,
            "Semantic error: The content contains semantic errors.",
            errors
        ));

    /// <summary>
    /// Server rozumí požadavku (pole jsou správná), ale požadavek obsahuje
    /// sémantickou chybu - například je rozměr hry 3x3 a obsahuje znak '@'
    /// </summary>
    public static IResult ValidationError(ValidationResult validationResult)
    {
        var errors = validationResult.GetValidationProblems();
        return ValidationError(
            validationResult.Errors
                .Select(e => SharedErrors.GeneralValidation(e.ErrorMessage))
                .ToArray()
        );
    }

    public static IResult Conflit(string reason, IReadOnlyCollection<Error>? Errors = null)
        => Results.Conflict(new ErrorResponse(409, $"Conflict: {reason}", Errors));

    public static IResult Conflit(IReadOnlyCollection<Error>? Errors = null)
        => Results.Conflict(new ErrorResponse(409, "Conflict: The resource already exists.", Errors));
}
