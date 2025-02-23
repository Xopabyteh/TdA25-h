using h.Contracts;
using Refit;
using System.Text.Json;

namespace h.Client.Services;

public static class HApiClientExtensions
{
    public static ErrorResponse ToErrorResponse(this ApiException? error)
    {
        if(error is null)
            throw new ArgumentNullException(nameof(error));

        if (error.Content is null)
            return new ErrorResponse(((int)error.StatusCode), error.Message, null);

        Console.WriteLine(error.Content);
        return JsonSerializer.Deserialize<ErrorResponse>(error.Content, AppJsonOptions.WithConverters);
    }
}
