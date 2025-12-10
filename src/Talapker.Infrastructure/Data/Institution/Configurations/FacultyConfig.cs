using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class FacultyConfig : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("faculties");

        builder.HasKey(f => f.Id);
        
        builder
            .Property(f => f.Name)
            .HasColumnName("name")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LocalizedText>(v, (JsonSerializerOptions?)null)!
            );

        builder
            .HasOne(f => f.Institution)
            .WithMany(u => u.Faculties)
            .HasForeignKey(f => f.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(f => f.EducationPrograms)
            .WithOne(ep => ep.Faculty)
            .HasForeignKey(ep => ep.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}