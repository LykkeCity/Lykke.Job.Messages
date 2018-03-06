using Lykke.Job.Messages.Contract.Emails;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface IBroadcastMailsRepository
    {
        Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group);
    }
}