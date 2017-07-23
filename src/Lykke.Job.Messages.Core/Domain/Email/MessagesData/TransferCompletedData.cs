namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class TransferCompletedData : IEmailMessageData
    {
        public string ClientName { get; set; }
        public double AmountFiat { get; set; }
        public double AmountLkk { get; set; }
        public double Price { get; set; }
        public string AssetId { get; set; }
        public string SrcBlockchainHash { get; set; }
        public string MessageId()
        {
            return "TransferCompletedEmail";
        }
    }
}
