namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class NoRefundDepositDoneData : IEmailMessageData
    {
        public const string EmailTemplateId = "NoRefundDepositDoneEmail";

        public string AssetBcnId { get; set; }
        public double Amount { get; set; }
    }
}
