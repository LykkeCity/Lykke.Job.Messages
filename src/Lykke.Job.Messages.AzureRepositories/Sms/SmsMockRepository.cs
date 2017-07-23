using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Sms;

namespace Lykke.Job.Messages.AzureRepositories.Sms
{
    public class SmsMockRepository : ISmsMockRepository
    {
        private readonly INoSQLTableStorage<SmsMessageMockEntity> _tableStorage;

        public SmsMockRepository(INoSQLTableStorage<SmsMessageMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertAsync(string phoneNumber, SmsMessage msg)
        {
            var newEntity = SmsMessageMockEntity.Create(phoneNumber, msg);
            return _tableStorage.InsertAsync(newEntity);
        }
    }
}