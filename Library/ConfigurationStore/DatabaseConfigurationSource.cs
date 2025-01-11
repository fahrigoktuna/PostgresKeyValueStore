using Microsoft.Extensions.Configuration;

namespace PostgresKeyValueStore.Library
{
    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseConfigurationSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(_serviceProvider);
        }
    }
}
