using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Data.CosmosDb
{
    public class CosmosDbContext : DbContext
    {
        public DbSet<Image> Images { get; set; }

        public CosmosDbContext()
        {
            Database.EnsureCreated();
        }



        #region Configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                "https://imagesdbgur.documents.azure.com:443/",
                "tnexrTBqAWt3oOAEZnGND38rtN2OODUKyySNQoZUkxiWHqQo3tgk5Hm0M9GjJ9VhffB7KYNvlNt4ACDbk99TOw==",
                databaseName: "imagesdbgur");


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Images");
        }

        #endregion
    }
}