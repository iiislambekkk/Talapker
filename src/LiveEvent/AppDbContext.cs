using Microsoft.EntityFrameworkCore;

namespace LiveEvent;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<BannedUser> BannedUsers => Set<BannedUser>();
}