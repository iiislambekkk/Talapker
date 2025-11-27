using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Talapker.Infrastructure.Data.Institution;
using Talapker.Infrastructure.Data.UserAccess;

namespace Talapker.Infrastructure.Data;

public class TalapkerDbContext : IdentityDbContext<ApplicationUser>
{
    public TalapkerDbContext(DbContextOptions<TalapkerDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<ApplicationRole> ApplicationRoles { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreScope> OpenIddictEntityFrameworkCoreScope { get; set; }
    
    public DbSet<City> Cities { get; set; }
    public DbSet<Institution.InstitutionEntity.Institution> Institutions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(typeof(TalapkerDbContext).Assembly);
    }
}
