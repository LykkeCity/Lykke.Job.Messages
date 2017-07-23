using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class EmailAttachmentsMockRepository : IEmailAttachmentsMockRepository
    {
        private readonly INoSQLTableStorage<EmailAttachmentsMockEntity> _tableStorage;

        public EmailAttachmentsMockRepository(INoSQLTableStorage<EmailAttachmentsMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertAsync(string emailMockId, string fileId, string fileName, string contentType)
        {
            var entity = EmailAttachmentsMockEntity.Create(emailMockId, fileId, fileName, contentType);
            await _tableStorage.InsertAsync(entity);
        }
    }
}