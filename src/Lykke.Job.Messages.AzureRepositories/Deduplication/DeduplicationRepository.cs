using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Job.Messages.Core.Domain.Deduplication;
using Lykke.SettingsReader;

namespace Lykke.Job.Messages.AzureRepositories.Deduplication
{
    public class DeduplicationRepository : IOperationMessagesDeduplicationRepository
    {
        private readonly INoSQLTableStorage<DeduplicationEntity> _storage;

        public static DeduplicationRepository Create(IReloadingManager<string> connectionString, ILogFactory logFactory)
        {
            var storage = AzureTableStorage<DeduplicationEntity>.Create(
                connectionString,
                "OperationMessagesDeduplication",
                logFactory);

            return new DeduplicationRepository(storage);
        }

        private DeduplicationRepository(INoSQLTableStorage<DeduplicationEntity> storage)
        {
            _storage = storage;
        }

        public Task InsertOrReplaceAsync(Guid operationId)
        {
            return _storage.InsertOrReplaceAsync(new DeduplicationEntity
            {
                PartitionKey = DeduplicationEntity.GetPartitionKey(operationId),
                RowKey = DeduplicationEntity.GetRowKey(operationId)
            });
        }

        public async Task<bool> IsExistsAsync(Guid operationId)
        {
            var partitionKey = DeduplicationEntity.GetPartitionKey(operationId);
            var rowKey = DeduplicationEntity.GetRowKey(operationId);

            return await _storage.GetDataAsync(partitionKey, rowKey) != null;
        }

        public Task TryRemoveAsync(Guid operationId)
        {
            var partitionKey = DeduplicationEntity.GetPartitionKey(operationId);
            var rowKey = DeduplicationEntity.GetRowKey(operationId);

            return _storage.DeleteIfExistAsync(partitionKey, rowKey);
        }
    }
}
