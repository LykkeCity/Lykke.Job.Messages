using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class EmailMockRepository : IEmailMockRepository
    {
        private readonly INoSQLTableStorage<EmailMockEntity> _tableStorage;

        public EmailMockRepository(INoSQLTableStorage<EmailMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ISmtpMailMock> InsertAsync(string address, EmailMessage msg)
        {
            var entity = EmailMockEntity.Create(address, msg);
            await _tableStorage.InsertAsync(entity);

            return entity;
        }
    }
}