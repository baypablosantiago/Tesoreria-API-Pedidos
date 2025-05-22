using Microsoft.EntityFrameworkCore;

namespace API_Pedidos.Models;

public class FundingRequestContext : DbContext
{
    public FundingRequestContext(DbContextOptions<FundingRequestContext> options)
        : base(options)
    {
    }

    public DbSet<FundingRequest> Requests { get; set; } = null!;
}