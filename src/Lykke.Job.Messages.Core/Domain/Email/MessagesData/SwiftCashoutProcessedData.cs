using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class SwiftCashoutProcessedData : IEmailMessageData
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string FullName { get; set; }
        public string Bic { get; set; }
        public string AccNum { get; set; }
        public string AccName { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }


        public string MessageId() => "SwiftCashoutProcessed";
    }
}
