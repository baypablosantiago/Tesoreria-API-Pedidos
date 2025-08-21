namespace API_Pedidos.Services;

public interface ILoginAuditService
{
    Task LogLoginAttemptAsync(string email, bool success, string ipAddress, string userAgent, string? failureReason = null);
}