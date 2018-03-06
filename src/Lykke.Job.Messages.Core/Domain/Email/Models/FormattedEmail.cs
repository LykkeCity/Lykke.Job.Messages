using Lykke.Service.EmailSender;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Core.Domain.Email.Models
{
    public class FormattedEmail
    {
        public EmailMessage EmailMessage { get; set; }
    }
}
