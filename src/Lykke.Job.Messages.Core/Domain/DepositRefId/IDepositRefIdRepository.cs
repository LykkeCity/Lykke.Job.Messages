using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.DepositRefId
{
    public interface IDepositRefIdRepository
    {
        void AddCodeAsync(string refCode, string clientId, string date, string code);
    }
}
