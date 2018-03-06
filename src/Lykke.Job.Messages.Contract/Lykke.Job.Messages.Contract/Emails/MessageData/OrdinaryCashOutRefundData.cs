namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class OrdinaryCashOutRefundData : BaseRefundData, IEmailMessageData
    {
        public const string QueueName = "OCashOutRefundEmail";

        public string AssetId { get; set; }
    }
}