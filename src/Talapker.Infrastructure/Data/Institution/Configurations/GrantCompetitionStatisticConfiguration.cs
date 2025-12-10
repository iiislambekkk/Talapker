using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talapker.Infrastructure.Data.Institution.InstitutionEntity;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class GrantCompetitionStatisticConfiguration : IEntityTypeConfiguration<GrantCompetitionStatistic>
{
    public void Configure(EntityTypeBuilder<GrantCompetitionStatistic> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasIndex(u => new { u.Year, u.EducationGroupId, u.CompetitionType }).IsUnique();
        
        builder.HasOne(u => u.EducationGroup)
            .WithMany(c => c.GrantCompetitionStatistics)
            .HasForeignKey(u => u.EducationGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.OwnsMany(e => e.Records, record => record.ToJson());
    }
}