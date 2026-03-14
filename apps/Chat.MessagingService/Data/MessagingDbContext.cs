using Chat.MessagingService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.MessagingService.Data;

public class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options) { }

    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var m = modelBuilder.Entity<MessageEntity>();

        m.ToTable("messages");
        m.HasKey(x => x.Id);

        m.Property(x => x.ConversationId).IsRequired().HasMaxLength(200);
        m.Property(x => x.SenderId).IsRequired().HasMaxLength(200);
        m.Property(x => x.Text).IsRequired().HasMaxLength(4000);
        m.Property(x => x.PersistedAtUtc).IsRequired();
        m.Property(x => x.ClientMessageId).IsRequired().HasMaxLength(100);

        m.HasIndex(x => x.ClientMessageId).IsUnique();

        m.HasIndex(x => new { x.ConversationId, x.PersistedAtUtc });
    }
}