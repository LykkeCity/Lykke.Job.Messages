using Lykke.Job.Messages.Core.Domain.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class EmailAttachmentsMockEntity : TableEntity, IEmailAttachmentsMock
    {
        public string EmailMockId => PartitionKey;
        public string AttachmentFileId => RowKey;
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public static EmailAttachmentsMockEntity Create(string emailMockId, string fileId,
            string fileName, string contentType)
        {
            return new EmailAttachmentsMockEntity
            {
                PartitionKey = GeneratePartition(emailMockId),
                RowKey = GenerateRowKey(fileId),
                FileName = fileName,
                ContentType = contentType
            };
        }

        private static string GenerateRowKey(string fileId)
        {
            return fileId;
        }

        private static string GeneratePartition(string emailMockId)
        {
            return emailMockId;
        }
    }
}