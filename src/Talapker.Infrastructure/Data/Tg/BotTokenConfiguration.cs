using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Tg;

public class BotTokenConfiguration : IEntityTypeConfiguration<BotToken>
{
    public void Configure(EntityTypeBuilder<BotToken> builder)
    {
        builder.ToTable("bot_tokens");
        
        builder.HasKey(x => x.BotId);
        
        builder.Property(x => x.BotId)
            .HasColumnName("bot_id");
            
        builder.Property(x => x.EncryptedToken)
            .HasColumnName("encrypted_token")
            .IsRequired();
            
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");
            
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");
    }
}