using Lykke.Job.Messages.Core.Domain.DepositRefId;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.AzureRepositories.DepositRefId
{
    public class DepositRefIdEntity : TableEntity, IDepositRefId
    {
        public string Date { get; set; }
        public string ClientId { get; set; }
        public string Code { get; set; }
    }
}
