using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace API_Pedidos.Models;

public class FundingRequestContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public FundingRequestContext(DbContextOptions<FundingRequestContext> options)
        : base(options)
    {
    }

    public DbSet<FundingRequest> Requests { get; set; } = null!;
}