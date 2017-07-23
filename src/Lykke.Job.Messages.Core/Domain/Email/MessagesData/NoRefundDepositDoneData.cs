namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class NoRefundDepositDoneData : IEmailMessageData
    {
        public string AssetBcnId { get; set; }
        public double Amount { get; set; }
        public string MessageId()
        {
            return "NoRefundDepositDoneEmail";
        }
    }
}
