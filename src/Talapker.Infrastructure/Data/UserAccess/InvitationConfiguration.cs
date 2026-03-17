using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Talapker.Infrastructure.Data.UserAccess;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.SecretCodeHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(i => i.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(i => i.Institution).WithMany().HasForeignKey(i => i.TenantId);

        builder.HasIndex(i => new { i.Email, i.TenantId, i.Role, i.Status });

        builder.HasIndex(i => i.SecretCodeHash);
    }
}