using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Messages.Email;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IBroadcastMailsRepository
    {
        Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group);
    }
}