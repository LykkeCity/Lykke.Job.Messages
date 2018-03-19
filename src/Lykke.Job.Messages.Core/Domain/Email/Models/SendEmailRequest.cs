using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.Email.Models
{
    public class SendEmailRequest<T> //where T : IEmailMessageData
    {
        public string PartnerId { get; set; }
        public string EmailAddress { get; set; }
        public T MessageData { get; set; }

        public SendEmailRequest(
            string emailAddress,
            string partnerId,
            T data)
        {
            EmailAddress = emailAddress;
            MessageData = data;
            PartnerId = partnerId;
        }
    }
}
