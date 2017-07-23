namespace Lykke.Job.Messages.Core.Domain.Email
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public EmailAttachment[] Attachments { get; set; }
    }
}