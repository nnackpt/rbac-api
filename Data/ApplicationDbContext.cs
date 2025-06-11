using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using RBACapi.Models;

namespace RBACapi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CM_APPLICATIONS> Applications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Env.Load();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfigurations.CM_APPLICATIONS_Configuration());
            base.OnModelCreating(modelBuilder);
        }
    }
}