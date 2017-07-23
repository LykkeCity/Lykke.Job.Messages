namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class BankCashInData : IEmailMessageData
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string ClientId { get; set; }

        public string MessageId()
        {
            return "BankCashInEmail";
        }
    }
}
