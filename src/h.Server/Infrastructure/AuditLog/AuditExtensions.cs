using System.Net;

namespace h.Server.Infrastructure.AuditLog;

public static class AuditExtensions
{
    public static RouteHandlerBuilder AuditMethod(
        this RouteHandlerBuilder builder,
        Func<EndpointFilterInvocationContext, FormattableString> actionMessageProvider)
    {
        return builder.AddEndpointFilter(async (context, next) => {
            var auditService = context.HttpContext.RequestServices.GetRequiredService<AuditLogService>();
            var ip = context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4() ?? IPAddress.None.MapToIPv4();
            
            var result = await next(context); // Execute the actual endpoint
            
            // Ensure success before logging
            if (result is IStatusCodeHttpResult statusCodeResult && !IsSuccessStatusCode(statusCodeResult.StatusCode))
            {
                return result;
            }

            var actionMessage = actionMessageProvider(context);
            
            await auditService.LogAsync(actionMessage, ip);

            return result;
          });
    }

    private static bool IsSuccessStatusCode(int? statusCode)
        => statusCode >= (int)HttpStatusCode.OK && statusCode <= (int)HttpStatusCode.PartialContent;
}
