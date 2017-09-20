using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Messages.Email.MessageData;

namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class SwiftCashoutProcessedData : IEmailMessageData
    {
        public string FullName { get; set; }
        public string Year { get; set; }


        public string MessageId() => "SwiftCashoutProcessed";
    }
}
