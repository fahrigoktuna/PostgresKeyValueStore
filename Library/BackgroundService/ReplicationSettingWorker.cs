using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.Replication.PgOutput.Messages;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication;
using Microsoft.Extensions.Hosting;

namespace PostgresKeyValueStore.Library
{
    public class ReplicationSettingWorker : BackgroundService
    {
        private readonly ILogger<ReplicationSettingWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ReplicationSettingWorker(ILogger<ReplicationSettingWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _configuration = _serviceProvider.GetRequiredService<IConfiguration>();

            var storeOptions = new StoreOptions();
            _configuration.GetSection(typeof(StoreOptions).Name).Bind(storeOptions);


            DatabaseConfigurationProvider? provider = null;

            if (_configuration is IConfigurationRoot configurationRoot)
            {
                provider = configurationRoot.Providers
                    .FirstOrDefault(p => p is DatabaseConfigurationProvider) as DatabaseConfigurationProvider;
            }

            try
            {
                await using var conn = new LogicalReplicationConnection(storeOptions.ConnectionString);
                await conn.Open(stoppingToken);

                var slot = new PgOutputReplicationSlot("config_slot");

                await foreach (var message in conn.StartReplication(
                    slot,
                    new PgOutputReplicationOptions("config_publication", 1),
                    stoppingToken))
                {
                    if (message is InsertMessage insertMessage)
                    {
                        string key = string.Empty;
                        string value = string.Empty;
                        await foreach (var column in insertMessage.NewRow.WithCancellation(stoppingToken))
                        {
                            if (column.GetFieldName().Contains("key"))
                            {
                                key = await column.GetTextReader().ReadToEndAsync();
                            }
                            else if (column.GetFieldName().Contains("value"))
                            {
                                value = await column.GetTextReader().ReadToEndAsync();
                            }
                        }

                        provider?.AddOrUpdate(key, value);
                    }
                    else if (message is UpdateMessage updateMessage)
                    {
                        string key = string.Empty;
                        string value = string.Empty;
                        await foreach (var column in updateMessage.NewRow.WithCancellation(stoppingToken))
                        {
                            if (column.GetFieldName().Contains("key"))
                            {
                                key = await column.GetTextReader().ReadToEndAsync();
                            }
                            else if (column.GetFieldName().Contains("value"))
                            {
                                value = await column.GetTextReader().ReadToEndAsync();
                            }
                        }
                        provider?.AddOrUpdate(key, value);

                    }
                    else if (message is KeyDeleteMessage deleteMessage)
                    {
                        await foreach (var column in deleteMessage.Key.WithCancellation(stoppingToken))
                        {
                            var value = column.Length > 0 ? await column.GetTextReader().ReadToEndAsync() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                provider?.Remove(value);
                            }
                        }
                    }

                    // Always set replication status
                    conn.SetReplicationStatus(message.WalEnd);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing replication messages.");
            }
        }
    }
}
