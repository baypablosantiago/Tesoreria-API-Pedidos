using System.Text.Json;
using API_Pedidos.Services;

namespace API_Pedidos.Middleware;

public class LoginAuditMiddleware
{
    private readonly RequestDelegate _next;

    public LoginAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILoginAuditService auditService)
    {
        if (context.Request.Path.StartsWithSegments("/login") && context.Request.Method == "POST")
        {
            context.Request.EnableBuffering();
            
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            try
            {
                var loginData = JsonSerializer.Deserialize<JsonElement>(body);
                var email = loginData.GetProperty("email").GetString() ?? "Unknown";
                
                var originalBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);

                var ipAddress = GetClientIpAddress(context);
                var userAgent = context.Request.Headers.UserAgent.ToString();
                
                bool success = context.Response.StatusCode == 200;
                string? failureReason = success ? null : $"HTTP {context.Response.StatusCode}";

                await auditService.LogLoginAttemptAsync(email, success, ipAddress, userAgent, failureReason);

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception)
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}