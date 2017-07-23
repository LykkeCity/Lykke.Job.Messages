using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Clients
{
    //TODO: remove and use IPersonalDataService
    public interface IPersonalDataRepository
    {
        Task<IPersonalData> GetAsync(string id);
    }
}