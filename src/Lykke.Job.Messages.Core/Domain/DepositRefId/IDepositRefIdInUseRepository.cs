using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.DepositRefId
{
    public interface IDepositRefIdInUseRepository
    {
        Task<IDepositRefIdInUse> GetRefIdAsync(string clientId, string date, string assetId);
    }
}
