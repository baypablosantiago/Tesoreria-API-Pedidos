using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace API_Pedidos.Models;

public class FundingRequestContext : IdentityDbContext
{
    public FundingRequestContext(DbContextOptions<FundingRequestContext> options)
        : base(options)
    {
    }

    public DbSet<FundingRequest> Requests { get; set; } = null!;
}