using Lykke.Job.Messages.Contract.Emails.MessageData;
using MessagePack;

namespace Lykke.Job.Messages.Workflow.Commands
{
    /// <summary>
    /// Command to send email message
    /// </summary>
    [MessagePackObject(keyAsPropertyName:true)]
    public class SendEmailCommand<T> where T : IEmailMessageData
    {
        public string PartnerId { get; set; }

        public string EmailAddress { get; set; }

        public T MessageData { get; set; }
    }
}
