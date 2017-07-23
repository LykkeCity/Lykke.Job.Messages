namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class NoRefundOCashOutData : IEmailMessageData
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string SrcBlockchainHash { get; set; }
        public string MessageId()
        {
            return "NoRefundOCashOutEmail";
        }
    }
}
