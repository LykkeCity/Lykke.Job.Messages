using System;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Commands
{
    public class SendEmailCommand
    {
        public SendEmailCommand()
        {
            CreationDate = DateTime.UtcNow;
        }

        public DateTime CreationDate { get; private set; }
        public string PartnerId { get; set; }
        public string EmailAddress { get; set; }
        public EmailMessage Message { get; set; }
    }
}