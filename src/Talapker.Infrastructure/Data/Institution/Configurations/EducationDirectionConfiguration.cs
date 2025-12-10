using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class EducationDirectionConfiguration : IEntityTypeConfiguration<EducationDirection>
{
    public void Configure(EntityTypeBuilder<EducationDirection> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasMany(e => e.EducationFields)
            .WithOne(f => f.EducationDirection)
            .HasForeignKey(f => f.EducationDirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(d => d.Name, n => RelationalOwnedNavigationBuilderExtensions.ToJson((OwnedNavigationBuilder)n));
    }
}