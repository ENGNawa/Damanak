using Damanak.Models;
using Microsoft.EntityFrameworkCore;

namespace Damanak.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Guarantee> Guarantees { get; set; }
        public DbSet<GuaranteeActivity> GuaranteeActivities { get; set; }
    }
}