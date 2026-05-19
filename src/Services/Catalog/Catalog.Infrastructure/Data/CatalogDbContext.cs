using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.ToTable("books");
            b.HasKey(x => x.Id);
            b.Property(x => x.Isbn).HasMaxLength(32).IsRequired();
            b.HasIndex(x => x.Isbn).IsUnique();
            b.Property(x => x.Title).HasMaxLength(180).IsRequired();
            b.Property(x => x.Author).HasMaxLength(120).IsRequired();
            b.Property(x => x.Genre).HasMaxLength(80).IsRequired();
            b.Property(x => x.Price).HasPrecision(12, 2);
            b.HasIndex(x => x.Title);
            b.HasIndex(x => x.Genre);
            b.HasIndex(x => x.Author);
        });
    }
}
