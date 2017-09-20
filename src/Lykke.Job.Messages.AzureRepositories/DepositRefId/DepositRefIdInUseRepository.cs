using AzureStorage;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.AzureRepositories.DepositRefId
{
    public class DepositRefIdInUseRepository : IDepositRefIdInUseRepository
    {
        private readonly INoSQLTableStorage<DepositRefIdInUseEntity> _tableStorage;

        public DepositRefIdInUseRepository(INoSQLTableStorage<DepositRefIdInUseEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IDepositRefIdInUse> GetRefIdAsync(string clientId, string date, string assetId)
        {

            string partitionKey = GeneratePartitionKey(date, clientId);

            var query = new TableQuery<DepositRefIdInUseEntity>().Where(TableQuery.GenerateFilterConditionForBool(nameof(DepositRefIdInUseEntity.isEmailSent), QueryComparisons.Equal, false));

            IEnumerable<DepositRefIdInUseEntity> availableRefIds = await _tableStorage.GetDataAsync(partitionKey, _ => _.isEmailSent == false && _.AssetId == assetId);
            if (availableRefIds.Count() > 0)
            {
                return availableRefIds.OrderByDescending(_ => _.Timestamp).First();
            }

            return null;
        }

        public string GeneratePartitionKey(string date, string clientId)
        {
            return $"{date}|{clientId}";
        }

        public string GenerateRowKey(string code)
        {
            return code;
        }


    }
}
