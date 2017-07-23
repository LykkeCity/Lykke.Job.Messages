using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.SwiftCredentials
{
    public interface ISwiftCredentialsRepository
    {
        Task<ISwiftCredentials> GetCredentialsAsync(string regulatorId, string assetId = null);
    }
}