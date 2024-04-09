using Microsoft.EntityFrameworkCore;

namespace Yritused.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Yritus> Yritused => Set<Yritus>();
        public DbSet<Osavotja> Osavotjad => Set<Osavotja>();
        public DbSet<YritusOsavotja> YritusOsavotjad => Set<YritusOsavotja>();
    }
}
