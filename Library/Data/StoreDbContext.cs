using Microsoft.EntityFrameworkCore;

namespace PostgresKeyValueStore.Library
{
    public class StoreDbContext : DbContext
    {
        public const string DefaultSchema = "store";

        public DbSet<Configuration> Configurations => Set<Configuration>();
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ConfigurationEntityTypeConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
