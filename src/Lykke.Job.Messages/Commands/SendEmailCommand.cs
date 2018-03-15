using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Commands
{
    public class SendEmailCommand
    {
        public string PartnerId { get; set; }
        public string EmailAddress { get; set; }
        public EmailMessage Message { get; set; }
    }
}