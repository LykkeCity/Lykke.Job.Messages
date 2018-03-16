namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class PlainTextBroadCastData : IEmailMessageData
    {
        public const string EmailTemplateId = "PlainTextBroadcast";

        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }
}