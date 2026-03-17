using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Assistant.Configurations;

public class KnowledgeEntryConfiguration : IEntityTypeConfiguration<KnowledgeEntry>
{
    public void Configure(EntityTypeBuilder<KnowledgeEntry> builder)
    {
        builder.ToTable("knowledge_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Question).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Answer).IsRequired().HasMaxLength(5000);
        builder.Property(x => x.Tags).HasColumnType("text[]");
        builder.HasOne(x => x.Institution)
            .WithMany()
            .HasForeignKey(x => x.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.InstitutionId);
        builder.HasIndex(x => x.SourceFileId);
    }
}