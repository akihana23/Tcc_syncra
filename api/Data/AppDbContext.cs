using Microsoft.EntityFrameworkCore;

using SocialListening.API.Models;

namespace SocialListening.API.Data
{
    public class AppDbContext
        : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options
        ) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<SearchHistory>
            SearchHistories
        { get; set; }

        public DbSet<BrandProfile>
            BrandProfiles
        { get; set; }

        public DbSet<BrandSnapshot>
            BrandSnapshots
        { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder
        )
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BrandProfile>()
                .HasIndex(brand => brand.Name);

            modelBuilder.Entity<BrandProfile>()
                .HasMany(brand => brand.Snapshots)
                .WithOne(snapshot => snapshot.BrandProfile)
                .HasForeignKey(snapshot => snapshot.BrandProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
