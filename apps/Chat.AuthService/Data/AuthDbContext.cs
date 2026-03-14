using Chat.AuthService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<AuthAccountEntity> Accounts => Set<AuthAccountEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var a = modelBuilder.Entity<AuthAccountEntity>();
        a.ToTable("auth_accounts");
        a.HasKey(x => x.Id);
        a.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        a.Property(x => x.Username).IsRequired().HasMaxLength(100);
        a.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        a.Property(x => x.PasswordHash).IsRequired();
        a.Property(x => x.CreatedAtUtc).IsRequired();
        a.Property(x => x.UpdatedAtUtc).IsRequired();

        a.HasIndex(x => x.UserId).IsUnique();
        a.HasIndex(x => x.Username).IsUnique();
    }
}
