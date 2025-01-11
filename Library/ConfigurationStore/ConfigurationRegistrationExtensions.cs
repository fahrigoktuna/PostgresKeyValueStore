using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Ardalis.GuardClauses;

namespace PostgresKeyValueStore.Library
{
    public static class ConfigurationRegistrationExtensions
    {
        static IConfigurationBuilder AddDatabaseConfiguration(this IConfigurationBuilder builder, IServiceProvider serviceProvider)
        {
            return builder.Add(new DatabaseConfigurationSource(serviceProvider));
        }

        public static IHostApplicationBuilder AddConfiguration([NotNull] this IHostApplicationBuilder builder)
        {
            var storeOptions = new StoreOptions();
            builder.Configuration.GetSection(typeof(StoreOptions).Name).Bind(storeOptions);

            //Todo connectionString kontrol et
            builder.Services.AddDbContext<StoreDbContext>(
                (sp, options) =>
                {
                    options
                        .UseNpgsql(
                                    storeOptions.ConnectionString,
                                    sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(
                                            typeof(StoreDbContext).Assembly.GetName().Name)
                                        .MigrationsHistoryTable(
                                            "efcore_store_migration_history", StoreDbContext.DefaultSchema);

                                        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                                    }
                                )
                                .UseSnakeCaseNamingConvention();

                }
            );
            var services = builder.Services.BuildServiceProvider();
            var storeDbContext =
                services.GetRequiredService<StoreDbContext>();

            storeDbContext.Database.MigrateAsync().ConfigureAwait(false).GetAwaiter().GetResult();


            builder.Services.AddScoped<IPostgresConfigurationService, PostgresConfigurationService>();
            builder.Configuration.AddDatabaseConfiguration(builder.Services.BuildServiceProvider());
            builder.Services.AddHostedService<ReplicationSettingWorker>();


            return builder;

        }

        public static IApplicationBuilder UseConfigurationUI([NotNull] this IApplicationBuilder app, string userName, string password, string uiaddress = "storedashboardui")
        {
            Guard.Against.NullOrEmpty(userName);
            Guard.Against.NullOrEmpty(password);
            Guard.Against.NullOrEmpty(uiaddress);

            app.UseMiddleware<StoreMiddleware>(userName, password, uiaddress);

            return app;
        }
    }
}
