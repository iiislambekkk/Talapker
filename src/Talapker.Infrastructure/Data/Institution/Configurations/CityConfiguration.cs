using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Id).IsUnique();
        
        builder.OwnsOne(d => d.Name, n => n.ToJson());
        
        builder.ToTable("Cities");
    }
}