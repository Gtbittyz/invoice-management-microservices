using EstoqueService.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Descricao).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Saldo).IsRequired();
        });

        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Codigo = "PRD-001", Descricao = "Caneta esferográfica azul", Saldo = 100 },
            new Produto { Id = 2, Codigo = "PRD-002", Descricao = "Caderno universitário 200fls", Saldo = 50 },
            new Produto { Id = 3, Codigo = "PRD-003", Descricao = "Monitor 24 polegadas", Saldo = 10 }
        );
    }
}
