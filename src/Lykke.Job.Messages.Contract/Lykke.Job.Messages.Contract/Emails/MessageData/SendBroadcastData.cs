using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SendBroadcastData<T> : IEmailMessageData
    {
        public string PartnerId { get; set; }
        public BroadcastGroup BroadcastGroup { get; set; }
        public T MessageData { get; set; }

        public static SendBroadcastData<T> Create(string partnerId, BroadcastGroup broadcastGroup, T msgData)
        {
            return new SendBroadcastData<T>
            {
                PartnerId = partnerId,
                BroadcastGroup = broadcastGroup,
                MessageData = msgData
            };
        }
    }
}
