using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.Institution.Configurations;

public class InstitutionConfiguration : IEntityTypeConfiguration<InstitutionEntity.Institution>
{
    public void Configure(EntityTypeBuilder<InstitutionEntity.Institution> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder
            .HasIndex(u => u.NationalCode)
            .IsUnique();
        
        builder
            .Property(u => u.NationalCode)
            .IsRequired();
        
        builder.HasOne(u => u.City)
            .WithMany(c => c.Institutions)
            .HasForeignKey(u => u.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.OwnsOne(d => d.Name, n => n.ToJson());
        builder.OwnsOne(d => d.Description, n => n.ToJson());
     
        builder.OwnsMany(u => u.Advantages, a =>
        {
            a.ToJson(); 

            a.OwnsOne(x => x.Title, t => t.ToJson());
            a.OwnsOne(x => x.Description, t => t.ToJson());
        });
        
        builder.HasMany(u => u.Faculties)
            .WithOne(f => f.Institution)
            .HasForeignKey(f => f.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ToTable("Institutions");
    }
}