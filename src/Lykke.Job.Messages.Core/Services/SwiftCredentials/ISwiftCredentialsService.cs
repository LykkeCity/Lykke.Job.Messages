using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Service.PersonalData.Contract.Models;

namespace Lykke.Job.Messages.Core.Services.SwiftCredentials
{
    public interface ISwiftCredentialsService
    {
        Task<ISwiftCredentials> GetCredentialsAsync(string assetId, double amount, IPersonalData personalData);
    }
}