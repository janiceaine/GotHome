using Microsoft.EntityFrameworkCore;

namespace GotHome.Models;

public class ApplicationContext : DbContext
{
    // DbSets to represent each table in the Db
    public DbSet<User> Users { get; set; }

    public DbSet<Profile> Profiles { get; set; }

    public ApplicationContext(DbContextOptions options)
        : base(options) { }
}
