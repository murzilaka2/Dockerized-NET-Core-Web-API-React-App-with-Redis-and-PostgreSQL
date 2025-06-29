using Microsoft.EntityFrameworkCore;
using VotingSystem.API.Models;

namespace VotingSystem.API.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<Vote>().Property(e=>e.Timestamp).HasDefaultValueSql("NOW()");
        }
    }
}
