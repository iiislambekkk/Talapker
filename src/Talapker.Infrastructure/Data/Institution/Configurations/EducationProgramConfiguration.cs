using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class EducationProgramConfiguration : IEntityTypeConfiguration<EducationProgram>
{
    public void Configure(EntityTypeBuilder<EducationProgram> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Faculty)
            .WithMany(f => f.EducationPrograms)
            .HasForeignKey(e => e.FacultyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EducationGroup)
            .WithMany(g => g.EducationPrograms)
            .HasForeignKey(e => e.EducationGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.OwnsOne(p => p.Name);
        builder.OwnsOne(p => p.Description);
        builder.OwnsOne(p => p.WorkPlaces);
        builder.OwnsOne(p => p.PractiseBases);

        builder.OwnsMany(p => p.Prices, prices =>
        {
            prices.ToJson();
        });

        builder.OwnsMany(p => p.Disciplines, disciplines =>
        {
            disciplines.ToJson();
            disciplines.OwnsOne(d => d.Name);
            disciplines.OwnsOne(d => d.Description);
        });
    }
}
