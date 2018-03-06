namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class KycOkCypData : IEmailMessageData
    {
        public const string QueueName = "WelcomeFxCypEmail";

        public string ClientId { get; set; }
        public string Year { get; set; }
    }
}
