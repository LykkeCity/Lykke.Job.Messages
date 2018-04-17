using Lykke.Job.Messages.Contract.Emails.MessageData;
using MessagePack;

namespace Lykke.Job.Messages.Contract.Commands
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

    /// <summary>
    /// Command to send email message
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class SendEmailCommand
    {
        public string PartnerId { get; set; }

        public string EmailAddress { get; set; }

        public string EmailTemplateId { get; set; }

        public string SerializedMessageData { get; set; }
    }
}
