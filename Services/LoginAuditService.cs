using Microsoft.AspNetCore.Identity;
using API_Pedidos.Models;

namespace API_Pedidos.Services;

public class LoginAuditService : ILoginAuditService
{
    private readonly FundingRequestContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginAuditService(FundingRequestContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task LogLoginAttemptAsync(string email, bool success, string ipAddress, string userAgent, string? failureReason = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        var audit = new LoginAudit
        {
            UserId = user?.Id ?? "Unknown",
            Email = email,
            LoginDate = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        };

        _context.LoginAudits.Add(audit);
        await _context.SaveChangesAsync();
    }
}