namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class UserRegisteredData : IEmailMessageData
    {
        public const string QueueName = "UserRegisteredBroadcast";

        public string ClientId { get; set; }
    }
}
