namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class FailedTransactionData : IEmailMessageData
    {
        public string TransactionId { get; set; }
        public string[] AffectedClientIds { get; set; }
        public string MessageId()
        {
            return "FailedTransactionBroadcast";
        }
    }
}
