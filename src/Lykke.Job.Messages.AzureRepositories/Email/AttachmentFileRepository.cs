using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Email;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class AttachmentFileRepository : IAttachmentFileRepository
    {
        private readonly IBlobStorage _blobStorage;
        private const string ContainerName = "mockattachments";

        public AttachmentFileRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> InsertAttachment(Stream stream)
        {
            var key = Guid.NewGuid().ToString("N");
            await _blobStorage.SaveBlobAsync(ContainerName, key, stream);
            return key;
        }

        public async Task<Stream> GetAttachment(string fileId)
        {
            return await _blobStorage.GetAsync(ContainerName, fileId);
        }
    }
}

