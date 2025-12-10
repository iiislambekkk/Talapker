using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class UntPairConfiguration : IEntityTypeConfiguration<UntPair>
{
    public void Configure(EntityTypeBuilder<UntPair> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(c => c.Id).IsUnique();
        
        builder
            .HasOne(u => u.FirstSubject)
            .WithMany()
            .HasForeignKey(u => u.FirstSubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(u => u.SecondSubject)
            .WithMany()
            .HasForeignKey(u => u.SecondSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.EducationGroups)
            .WithMany(g => g.UntSubjectsPairs);
        
        builder.ToTable("UntPairs");
    }
}