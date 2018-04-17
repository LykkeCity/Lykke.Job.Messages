using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Contract.Emails;
using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class BroadcastMailsRepository : IBroadcastMailsRepository
    {
        private readonly INoSQLTableStorage<BroadcastMailEntity> _tableStorage;

        public BroadcastMailsRepository(INoSQLTableStorage<BroadcastMailEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group)
        {
            return await _tableStorage.GetDataAsync(BroadcastMailEntity.GeneratePartitionKey(group));
        }
    }
}