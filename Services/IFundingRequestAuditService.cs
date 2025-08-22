namespace API_Pedidos.Services;

public interface IFundingRequestAuditService
{
    Task LogCreateAsync(long fundingRequestId, string userId, string userEmail);
    Task LogUpdateAsync(long fundingRequestId, string userId, string userEmail, string fieldChanged, string? oldValue, string? newValue);
    Task LogStatusChangeAsync(long fundingRequestId, string userId, string userEmail, string action, string description);
    Task LogCommentAsync(long fundingRequestId, string userId, string userEmail, string comment);
    Task LogPaymentUpdateAsync(long fundingRequestId, string userId, string userEmail, double oldPayment, double newPayment);
}