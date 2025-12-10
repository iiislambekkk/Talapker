using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class EducationFieldConfiguration : IEntityTypeConfiguration<EducationField>
{
    public void Configure(EntityTypeBuilder<EducationField> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasMany<EducationGroup>()
            .WithOne(g => g.EducationField)
            .HasForeignKey(g => g.EducationFieldId)
            .OnDelete(DeleteBehavior.Restrict);
      
        builder.OwnsOne(d => d.Name, n => n.ToJson());
        
        builder.HasIndex(e => e.NationalCode).IsUnique();
        builder.Property(e => e.NationalCode).IsRequired();
    }
}
