using Lykke.Job.Messages.Core.Domain.DepositRefId;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.AzureRepositories.DepositRefId
{
    public class DepositRefIdInUseEntity : TableEntity, IDepositRefIdInUse
    {
        public string Date { get; set; }
        public string ClientId { get; set; }
        public string Code { get; set; }
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public bool isEmailSent { get; set; }
    }
}
