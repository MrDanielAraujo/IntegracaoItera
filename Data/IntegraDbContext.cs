using IntegracaoItera.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace IntegracaoItera.Data;

public class IntegraDbContext : DbContext
{
    public IntegraDbContext(DbContextOptions<IntegraDbContext> options) : base(options)
    {
    }

    public DbSet<Documento> Documento { get; set; } = null!;

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
