namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class TransferCompletedData : IEmailMessageData
    {
        public const string EmailTemplateId = "TransferCompletedEmail";

        public string ClientName { get; set; }
        public double AmountFiat { get; set; }
        public double AmountLkk { get; set; }
        public double Price { get; set; }
        public string AssetId { get; set; }
        public string SrcBlockchainHash { get; set; }
    }
}
