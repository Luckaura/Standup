using Komikai.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Komikai.Data
{
    public class ForumDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public DbSet<Comedian> Comedians { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public ForumDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString(("PostgreSQL")));
        }
    }
}
