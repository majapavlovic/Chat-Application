using Chat.ConversationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.ConversationService.Data;

public class ConversationDbContext : DbContext
{
    public ConversationDbContext(DbContextOptions<ConversationDbContext> options) : base(options) { }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> Participants => Set<ConversationParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var c = modelBuilder.Entity<Conversation>();
        c.ToTable("conversations");
        c.HasKey(x => x.Id);
        c.Property(x => x.Type).IsRequired();
        c.Property(x => x.Name).HasMaxLength(200);
        c.Property(x => x.CreatedAtUtc).IsRequired();
        c.HasMany(x => x.Participants)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId);

        var p = modelBuilder.Entity<ConversationParticipant>();
        p.ToTable("conversation_participants");
        p.HasKey(x => new { x.ConversationId, x.UserId });
        p.Property(x => x.UserId).IsRequired().HasMaxLength(200);
        p.Property(x => x.JoinedAtUtc).IsRequired();

        p.HasIndex(x => x.UserId);
    }
}
