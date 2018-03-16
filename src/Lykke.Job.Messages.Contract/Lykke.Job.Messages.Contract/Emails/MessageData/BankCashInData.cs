namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class BankCashInData : IEmailMessageData
    {
        public const string EmailTemplateId = "BankCashInEmail";

        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string ClientId { get; set; }
    }
}
