namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class VisaShippingInfoData : IEmailMessageData
    {
        public const string EmailTemplateId = "VisaShippingInfo";
        public string TrackingId { get; set; }
        public string TrackingUrl { get; set; }
        public string Year { get; set; }
    }
}
