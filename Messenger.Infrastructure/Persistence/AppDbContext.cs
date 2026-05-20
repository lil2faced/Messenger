using Messenger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Login).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Nickname).IsUnique();
            entity.Property(u => u.Login).HasMaxLength(50);
            entity.Property(u => u.Email).HasMaxLength(100);
            entity.Property(u => u.Nickname).HasMaxLength(30);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Text).HasMaxLength(2000);
            entity.HasOne(m => m.Sender)
                  .WithMany()
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.Recipient)
                  .WithMany()
                  .HasForeignKey(m => m.RecipientId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.RepliedToMessage)
                  .WithMany()
                  .HasForeignKey(m => m.RepliedToMessageId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(m => new { m.SenderId, m.CreatedAt });
            entity.HasIndex(m => new { m.RecipientId, m.Status });
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileUrl).HasMaxLength(500);
            entity.Property(a => a.Type).HasMaxLength(10);
            entity.HasOne(a => a.Message)
                  .WithMany(m => m.Attachments)
                  .HasForeignKey(a => a.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}