namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class VisaUpgradeLimitsData : IEmailMessageData
    {
        public const string EmailTemplateId = "VisaUpgradeLimits";
        public string Title { get; set; }
        public string Message { get; set; }
        public string Year { get; set; }
    }
}
