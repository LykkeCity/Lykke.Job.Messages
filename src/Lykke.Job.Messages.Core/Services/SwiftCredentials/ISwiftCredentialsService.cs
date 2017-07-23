using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;

namespace Lykke.Job.Messages.Core.Services.SwiftCredentials
{
    public interface ISwiftCredentialsService
    {
        Task<ISwiftCredentials> GetCredentialsAsync(string assetId, IPersonalData personalData);
    }
}