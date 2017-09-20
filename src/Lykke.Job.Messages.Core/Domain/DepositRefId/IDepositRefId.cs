using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.DepositRefId
{
    public interface IDepositRefId
    {
        string Date { get; set; }
        string ClientId { get; set; }
        string Code { get; set; }
    }
}
