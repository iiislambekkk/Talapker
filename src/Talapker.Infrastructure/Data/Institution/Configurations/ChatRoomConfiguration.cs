using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Prospect)
            .WithMany()
            .HasForeignKey(r => r.ProspectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Ambassador)
            .WithMany()
            .HasForeignKey(r => r.AmbassadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Institution)
            .WithMany()
            .HasForeignKey(r => r.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ProspectId, r.AmbassadorId, r.InstitutionId })
            .IsUnique(); // один чат между парой в рамках института

        builder.HasMany(r => r.Messages)
            .WithOne(m => m.ChatRoom)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Text)
            .HasMaxLength(4000);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.ChatRoomId);
        builder.HasIndex(m => m.SentAt);
    }
}