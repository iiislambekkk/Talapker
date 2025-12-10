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
        
        builder.OwnsOne(d => d.Name, n => n.ToJson());
        builder.OwnsOne(d => d.Description, n => n.ToJson());
    }
}
