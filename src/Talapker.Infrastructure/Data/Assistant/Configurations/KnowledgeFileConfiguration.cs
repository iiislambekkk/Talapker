using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Assistant.Configurations;

public class KnowledgeFileConfiguration : IEntityTypeConfiguration<KnowledgeFile>
{
    public void Configure(EntityTypeBuilder<KnowledgeFile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.HasOne(x => x.Institution)
            .WithMany()
            .HasForeignKey(x => x.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Entries)
            .WithOne(x => x.SourceFile)
            .HasForeignKey(x => x.SourceFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.InstitutionId);
    }
}