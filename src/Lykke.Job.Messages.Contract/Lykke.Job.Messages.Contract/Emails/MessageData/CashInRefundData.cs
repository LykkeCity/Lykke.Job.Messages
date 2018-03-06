namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class CashInRefundData : BaseRefundData, IEmailMessageData
    {
        public const string QueueName = "CashInRefundEmail";
    }
}