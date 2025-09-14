using Microsoft.EntityFrameworkCore;
using ProductApi.Api.Models;

namespace ProductApi.Api.Data;

public class ProductApiDbContext : DbContext
{
    public ProductApiDbContext(DbContextOptions<ProductApiDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Create SQL Server sequence for distributed-safe ProductId generation
        modelBuilder.HasSequence<int>("ProductIdSequence")
            .StartsAt(100001)
            .IncrementsBy(1);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductApiDbContext).Assembly);
    }
}

