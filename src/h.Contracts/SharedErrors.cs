namespace h.Contracts;

/// <summary>
/// Errors shared across server and client.
/// Errors created via <see cref="ErrorOr"/> and each has a code created using their "<see cref="nameof(Type)"/>",
/// which shall be used for comparing, error handling or <b>message localization</b>.
/// </summary>
public static partial class SharedErrors;
