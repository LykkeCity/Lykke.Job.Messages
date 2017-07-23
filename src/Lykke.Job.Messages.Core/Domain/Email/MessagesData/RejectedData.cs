namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class RejectedData : IEmailMessageData {
        public string MessageId()
        {
            return "RejectedEmail";
        }
    }
}
