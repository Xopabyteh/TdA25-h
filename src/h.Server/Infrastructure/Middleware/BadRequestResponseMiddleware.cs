using h.Contracts;
using System.Text.Json;

namespace h.Server.Infrastructure.Middleware;

/// <summary>
/// Catches the default <see cref="BadHttpRequestException"/> and returns
/// a custom 400 Bad Request response.
/// </summary>
public class BadRequestResponseMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            var error = new ErrorResponse(
                400,
                $"Bad request: {ex.Message}");

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, error, options: JsonSerializerOptions.Web);
        }
    }
}
