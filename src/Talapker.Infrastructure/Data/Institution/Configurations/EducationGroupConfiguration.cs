using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class EducationGroupConfiguration : IEntityTypeConfiguration<EducationGroup>
{
    public void Configure(EntityTypeBuilder<EducationGroup> builder)
    {
        builder.HasKey(e => e.Id);
        
     
        builder.OwnsOne(d => d.Name, n => n.ToJson());
        
        builder
            .HasOne(e => e.EducationField)
            .WithMany(e => e.EducationGroups)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasMany(e => e.EducationPrograms)
            .WithOne(e => e.EducationGroup)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.NationalCode).IsUnique();
        builder.Property(e => e.NationalCode).IsRequired();

        builder.HasMany(e => e.UntSubjectsPairs)
            .WithMany(p => p.EducationGroups);
    }
}
