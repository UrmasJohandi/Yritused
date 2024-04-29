using Microsoft.EntityFrameworkCore;

namespace Yritused.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Yritus> Yritused { get; set; }
        public DbSet<Osavotja> Osavotjad { get; set; }
        public DbSet<YritusOsavotja> YritusOsavotjad { get; set; }
        public DbSet<Seadistus> Seadistused { get; set; }
    }
}
