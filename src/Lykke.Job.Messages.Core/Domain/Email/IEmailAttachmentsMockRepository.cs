using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IEmailAttachmentsMockRepository
    {
        Task InsertAsync(string emailMockId, string fileId, string fileName, string contentType);
    }
}