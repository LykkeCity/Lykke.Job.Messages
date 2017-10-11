using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Messages.Core.Domain.DepositRefId
{
    public interface IDepositRefIdRepository
    {
        Task AddCodeAsync(string refCode, string clientId, string date, string code);
    }
}
