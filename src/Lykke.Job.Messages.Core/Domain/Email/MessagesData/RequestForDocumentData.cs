using Lykke.Messages.Email.MessageData;

namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class RequestForDocumentData : IEmailMessageData
    {
        public string MessageId()
        {
            return "RequestForDocument";
        }

        public string ClientId { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
    }
}
