namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class OrdinaryCashOutRefundData : BaseRefundData, IEmailMessageData
    {
        public const string EmailTemplateId = "OCashOutRefundEmail";

        public string AssetId { get; set; }
    }
}