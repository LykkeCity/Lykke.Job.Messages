namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class MyLykkeCashInData : IEmailMessageData
    {
        public const string QueueName = "MyLykkeCashIn";

        public string ConversionWalletAddress { get; set; }
        public double Amount { get; set; }
        public double LkkAmount { get; set; }
        public double Price { get; set; }
        public string AssetId { get; set; }
    }
}
