using Microsoft.EntityFrameworkCore;

namespace IntegracaoItera.Data;

public class IntegraDbContext : DbContext
{
    public IntegraDbContext(DbContextOptions<IntegraDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
