using Microsoft.EntityFrameworkCore;

namespace PostgresKeyValueStore.Library
{
    public interface IPostgresConfigurationService
    {
        Task<Dictionary<string, string>> GetAllConfigurationsAsync();
    }
    public class PostgresConfigurationService : IPostgresConfigurationService
    {
        private readonly StoreDbContext _context;

        public PostgresConfigurationService(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetAllConfigurationsAsync()
        {
            return await _context.Configurations.AsNoTracking()
                .ToDictionaryAsync(c => c.Key, c => c.Value);
        }
    }
}
