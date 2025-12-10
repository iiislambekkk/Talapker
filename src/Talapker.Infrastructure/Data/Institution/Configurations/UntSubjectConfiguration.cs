using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class UntSubjectConfiguration : IEntityTypeConfiguration<UntSubject>
{
    public void Configure(EntityTypeBuilder<UntSubject> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(c => c.Id).IsUnique();
        
        builder.Property(u => u.Name)
            .HasColumnName("description")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LocalizedText>(v, (JsonSerializerOptions?)null)!
            );
        
        builder.ToTable("UntSubjects");
    }
}