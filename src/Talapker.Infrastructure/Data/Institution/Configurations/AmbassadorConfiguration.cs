using Elastic.CommonSchema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class AmbassadorConfiguration : IEntityTypeConfiguration<Ambassador>
{
    public void Configure(EntityTypeBuilder<Ambassador> builder)
    {
        builder.HasOne<EducationProgram>(a => a.EducationProgram).WithMany().HasForeignKey(a => a.EducationalProgramId);
        builder.HasOne<Institution>(a => a.Institution).WithMany().HasForeignKey(a => a.InstitutionId);
        builder.HasOne<ApplicationUser>(a => a.User).WithOne().HasForeignKey<Ambassador>(a => a.UserId);
    }
}