namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class FailedTransactionData : IEmailMessageData
    {
        public const string EmailTemplateId = "FailedTransactionBroadcast";

        public string TransactionId { get; set; }
        public string[] AffectedClientIds { get; set; }
    }
}
