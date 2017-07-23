using System.IO;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IAttachmentFileRepository
    {
        Task<string> InsertAttachment(Stream stream);
        Task<Stream> GetAttachment(string fileId);
    }
}