namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class UserRegisteredData : IEmailMessageData
    {
        public string ClientId { get; set; }
        public string MessageId()
        {
            return "UserRegisteredBroadcast";
        }
    }
}
