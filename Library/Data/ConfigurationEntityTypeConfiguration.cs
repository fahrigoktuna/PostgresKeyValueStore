using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PostgresKeyValueStore.Library
{
    public class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<Configuration>
    {
        public void Configure(EntityTypeBuilder<Configuration> builder)
        {
            builder.ToTable("configuration", StoreDbContext.DefaultSchema);
            builder.HasKey(x => x.Key);
            builder.Property(x => x.Key).IsRequired();
            builder.Property(x => x.Value).IsRequired();
        }
    }
}
