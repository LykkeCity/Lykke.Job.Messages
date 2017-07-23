using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Clients;

namespace Lykke.Job.Messages.AzureRepositories.Clients
{
    public class PersonalDataRepository : IPersonalDataRepository
    {
        private readonly INoSQLTableStorage<PersonalDataEntity> _tableStorage;

        public PersonalDataRepository(INoSQLTableStorage<PersonalDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IPersonalData> GetAsync(string id)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}