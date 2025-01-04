using CodingCleanProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CodingCleanProject.Data
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions dbContext) : base(dbContext) { }
        public DbSet<Stock> stocks { get; set; }
        public DbSet<Comment> comments { get; set; }

    }
}
