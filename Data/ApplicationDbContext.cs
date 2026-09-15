using ExternalIdDemo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExternalIdDemo.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserMfa> UserMfas => Set<UserMfa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserMfa>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntraObjectId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.TotpSecretEncrypted)
                .IsRequired();

            entity.HasIndex(x => x.EntraObjectId)
                .IsUnique();
        });
    }
}
