namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SwiftCashoutRequestedData : IEmailMessageData
    {
        public const string EmailTemplateId = "SwiftCashoutRequested";

        public string FullName { get; set; }
        public string Year { get; set; }
    }
}
