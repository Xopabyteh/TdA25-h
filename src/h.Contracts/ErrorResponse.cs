namespace h.Contracts;
public readonly record struct ErrorResponse(
    int Code,
    string Message,
    Dictionary<string, string[]>? Errors = null);
