using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SendEmailData<T> where T : IEmailMessageData
    {
        public string PartnerId { get; set; }
        public string EmailAddress { get; set; }
        public T MessageData { get; set; }

        public static SendEmailData<T> Create(string partnerId, string emailAddress, T msgData)
        {
            return new SendEmailData<T>
            {
                PartnerId = partnerId,
                EmailAddress = emailAddress,
                MessageData = msgData
            };
        }
    }
}
