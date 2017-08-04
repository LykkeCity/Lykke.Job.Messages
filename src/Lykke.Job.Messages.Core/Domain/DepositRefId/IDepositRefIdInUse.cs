using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.DepositRefId
{
    public interface IDepositRefIdInUse
    {
        string Date { get; set; }
        string ClientId { get; set; }
        string Code { get; set; }
        string AssetId { get; set; }
        double Amount { get; set; }
        bool isEmailSent { get; set; }
    }
}
