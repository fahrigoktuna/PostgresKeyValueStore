using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace PostgresKeyValueStore.Library.Data
{
    public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
    {
        public StoreDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

         

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory ?? "")
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var connectionStringSectionValue = configuration.GetSection("StoreOptions:ConnectionString").Value;

            var optionsBuilder = new DbContextOptionsBuilder<StoreDbContext>()
               .UseNpgsql(
                   connectionStringSectionValue,
                   sqlOptions =>
                   {
                       sqlOptions.MigrationsAssembly(typeof(StoreDbContext).Assembly.FullName);
                       sqlOptions.MigrationsHistoryTable(
                                   $"efcore_{nameof(StoreDbContext)}_migration_history", StoreDbContext.DefaultSchema);
                       sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                   }
               )
               .UseSnakeCaseNamingConvention();

#pragma warning disable CS8603 // Possible null reference return.
            return Activator.CreateInstance(typeof(StoreDbContext), optionsBuilder.Options) as StoreDbContext;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
