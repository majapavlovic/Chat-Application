using Chat.UserService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserConnectionEntity> Connections => Set<UserConnectionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var u = modelBuilder.Entity<UserEntity>();
        u.ToTable("users");
        u.HasKey(x => x.Id);
        u.Property(x => x.Id).HasMaxLength(200);
        u.Property(x => x.Username).IsRequired().HasMaxLength(100);
        u.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        u.Property(x => x.IsOnline).IsRequired();
        u.Property(x => x.LastSeenAtUtc);
        u.Property(x => x.CreatedAtUtc).IsRequired();
        u.HasIndex(x => x.Username).IsUnique();

        var c = modelBuilder.Entity<UserConnectionEntity>();
        c.ToTable("user_connections");
        c.HasKey(x => new { x.UserAId, x.UserBId });
        c.Property(x => x.UserAId).IsRequired().HasMaxLength(200);
        c.Property(x => x.UserBId).IsRequired().HasMaxLength(200);
        c.Property(x => x.RequestedByUserId).IsRequired().HasMaxLength(200);
        c.Property(x => x.Status).IsRequired();
        c.Property(x => x.UpdatedAtUtc).IsRequired();
        c.HasIndex(x => x.UserAId);
        c.HasIndex(x => x.UserBId);
    }
}
