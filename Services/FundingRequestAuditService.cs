using API_Pedidos.Models;

namespace API_Pedidos.Services;

public class FundingRequestAuditService : IFundingRequestAuditService
{
    private readonly FundingRequestContext _context;

    public FundingRequestAuditService(FundingRequestContext context)
    {
        _context = context;
    }

    public async Task LogCreateAsync(long fundingRequestId, string userId, string userEmail)
    {
        var audit = new FundingRequestAudit
        {
            FundingRequestId = fundingRequestId,
            UserId = userId,
            UserEmail = userEmail,
            ChangeDate = DateTime.UtcNow,
            Action = "CREATE",
            Description = "Solicitud de financiamiento creada"
        };

        _context.FundingRequestAudits.Add(audit);
        await _context.SaveChangesAsync();
    }

    public async Task LogUpdateAsync(long fundingRequestId, string userId, string userEmail, string fieldChanged, string? oldValue, string? newValue)
    {
        var audit = new FundingRequestAudit
        {
            FundingRequestId = fundingRequestId,
            UserId = userId,
            UserEmail = userEmail,
            ChangeDate = DateTime.UtcNow,
            Action = "UPDATE",
            FieldChanged = fieldChanged,
            OldValue = oldValue,
            NewValue = newValue,
            Description = $"Campo '{fieldChanged}' actualizado de '{oldValue}' a '{newValue}'"
        };

        _context.FundingRequestAudits.Add(audit);
        await _context.SaveChangesAsync();
    }

    public async Task LogStatusChangeAsync(long fundingRequestId, string userId, string userEmail, string action, string description)
    {
        var audit = new FundingRequestAudit
        {
            FundingRequestId = fundingRequestId,
            UserId = userId,
            UserEmail = userEmail,
            ChangeDate = DateTime.UtcNow,
            Action = action,
            Description = description
        };

        _context.FundingRequestAudits.Add(audit);
        await _context.SaveChangesAsync();
    }

    public async Task LogCommentAsync(long fundingRequestId, string userId, string userEmail, string comment)
    {
        var audit = new FundingRequestAudit
        {
            FundingRequestId = fundingRequestId,
            UserId = userId,
            UserEmail = userEmail,
            ChangeDate = DateTime.UtcNow,
            Action = "ADD_COMMENT",
            NewValue = comment,
            Description = "Comentario agregado por tesorería"
        };

        _context.FundingRequestAudits.Add(audit);
        await _context.SaveChangesAsync();
    }

    public async Task LogPaymentUpdateAsync(long fundingRequestId, string userId, string userEmail, double oldPayment, double newPayment)
    {
        var audit = new FundingRequestAudit
        {
            FundingRequestId = fundingRequestId,
            UserId = userId,
            UserEmail = userEmail,
            ChangeDate = DateTime.UtcNow,
            Action = "UPDATE_PAYMENT",
            FieldChanged = "PartialPayment",
            OldValue = oldPayment.ToString("C"),
            NewValue = newPayment.ToString("C"),
            Description = $"Pago parcial actualizado de {oldPayment:C} a {newPayment:C}"
        };

        _context.FundingRequestAudits.Add(audit);
        await _context.SaveChangesAsync();
    }
}