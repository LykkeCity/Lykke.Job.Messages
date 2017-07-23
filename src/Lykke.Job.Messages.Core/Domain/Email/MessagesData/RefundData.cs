namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class BaseRefundData 
    {
        public double Amount { get; set; }
        public string SrcBlockchainHash { get; set; }
        public string RefundTransaction { get; set; }
        public int ValidDays { get; set; }
    }

    public class CashInRefundData : BaseRefundData, IEmailMessageData
    {
        public string MessageId()
        {
            return "CashInRefundEmail";
        }
    }

    public class SwapRefundData : BaseRefundData, IEmailMessageData
    {
        public string MessageId()
        {
            return "SwapRefundEmail";
        }
    }

    public class OrdinaryCashOutRefundData : BaseRefundData, IEmailMessageData
    {
        public string AssetId { get; set; }
        public string MessageId()
        {
            return "OCashOutRefundEmail";
        }
    }
}
