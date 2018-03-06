namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SwiftCashoutProcessedData : IEmailMessageData
    {
        public const string EmailTemplateId = "SwiftCashoutProcessed";

        public string FullName { get; set; }
        public string Year { get; set; }
    }
}
