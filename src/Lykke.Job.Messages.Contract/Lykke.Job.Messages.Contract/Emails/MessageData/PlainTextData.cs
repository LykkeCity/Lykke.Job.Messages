namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class PlainTextData : IEmailMessageData
    {
        public const string QueueName = "PlainTextEmail";

        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }
}

