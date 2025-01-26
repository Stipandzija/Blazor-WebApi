using CodingCleanProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection.Emit;


namespace CodingCleanProject.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions dbContext) : base(dbContext) { }
        public DbSet<Stock> stocks { get; set; }
        public DbSet<Comment> comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserStock> Portofolios { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.Entity<UserStock>(x => x.HasKey(p => new { p.UserId, p.StockId }));//strani kljuc
            builder.Entity<UserStock>()
                .HasOne(x => x.User)
                .WithMany(x => x.userStocks)
                .HasForeignKey(p => p.UserId);
            builder.Entity<UserStock>()
                .HasOne(x => x.Stock)
                .WithMany(x => x.userStocks)
                .HasForeignKey(p => p.StockId);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name ="Admin",
                    NormalizedName="ADMIN"
                },
                new IdentityRole
                {
                    Name ="User",
                    NormalizedName="USER"
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);

            builder.Entity<Stock>()
                .Property(s => s.LastDiv)
                .HasColumnType("decimal(18, 2)");
            builder.Entity<Stock>()
                .Property(s => s.Purchase)
                .HasColumnType("decimal(18, 2)");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }


    }
}
