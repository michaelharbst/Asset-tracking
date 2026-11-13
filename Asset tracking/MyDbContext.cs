using Microsoft.EntityFrameworkCore;

namespace Asset_tracking
{
    public class MyDbContext : DbContext
    {
        private readonly string _connectionString = @"Server=localhost\SQLEXPRESS;Database=asset_Tracking;Trusted_Connection=True;TrustServerCertificate=True;";
        public DbSet<Asset> Assets { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .ToTable("Assets");

            modelBuilder.Entity<Asset>()
                .HasDiscriminator(asset => asset.AssetType)
                .HasValue<Smartphone>(AssetType.Smartphone)
                .HasValue<Computer>(AssetType.Computer);

            modelBuilder.Entity<Asset>()
                .ComplexProperty(asset => asset.Price, price =>
                {
                    price.Property(p => p.PriceUSD)
                        .HasColumnName("PriceUSD");

                    price.Property(p => p.Currency)
                        .HasColumnName("Currency");
                });
        }
    }

}