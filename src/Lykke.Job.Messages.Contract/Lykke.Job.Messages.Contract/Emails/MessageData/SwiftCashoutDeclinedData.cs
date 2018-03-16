namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SwiftCashoutDeclinedData : IEmailMessageData
    {
        public const string EmailTemplateId = "SwiftCashoutDeclined";

        public string FullName { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
    }
}
