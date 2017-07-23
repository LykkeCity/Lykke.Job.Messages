using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;

namespace Lykke.Job.Messages.AzureRepositories.SwiftCredentials
{
    public class SwiftCredentialsRepository : ISwiftCredentialsRepository
    {
        private readonly INoSQLTableStorage<SwiftCredentialsEntity> _tableStorage;

        public SwiftCredentialsRepository(INoSQLTableStorage<SwiftCredentialsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string regulatorId, string assetId = null)
        {
            var partitionKey = SwiftCredentialsEntity.GeneratePartitionKey(regulatorId);
            var rowKey = SwiftCredentialsEntity.GenerateRowKey(assetId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}