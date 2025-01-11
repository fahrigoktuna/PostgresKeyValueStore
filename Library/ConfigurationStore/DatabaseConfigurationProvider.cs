using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PostgresKeyValueStore.Library
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        readonly IServiceProvider _serviceProvider;

        public DatabaseConfigurationProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Load()
        {
            using var scope = _serviceProvider.CreateScope();
            var configurationService = scope.ServiceProvider.GetRequiredService<IPostgresConfigurationService>();
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            Data = configurationService.GetAllConfigurationsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public void AddOrUpdate(string key, string value)
        {
            if (Data.ContainsKey(key))
                Data[key] = value;
            else
                Data.Add(key, value);

            OnReload();
        }

        public void Remove(string key)
        {
            Data.Remove(key);

            OnReload();
        }
    }
}
