using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.Email.Models
{
    public class SendEmailRequest<T>
    {
        public string PartnerId { get; set; }
        public string EmailAddress { get; set; }
        public T MessageData { get; set; }
    }
}
