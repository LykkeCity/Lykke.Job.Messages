using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Regulator
{
    public interface IRegulatorRepository
    {
        Task<IRegulator> GetByIdOrDefaultAsync(string id);
    }
}