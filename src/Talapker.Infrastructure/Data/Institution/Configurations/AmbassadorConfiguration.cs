using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class AmbassadorConfiguration : IEntityTypeConfiguration<Ambassador>
{
    public void Configure(EntityTypeBuilder<Ambassador> builder)
    {
        builder.HasOne<EducationProgram>(a => a.EducationProgram).WithMany().HasForeignKey(a => a.EducationalProgramId);
    }
}