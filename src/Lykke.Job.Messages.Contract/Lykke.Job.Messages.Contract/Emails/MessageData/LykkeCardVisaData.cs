namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class LykkeCardVisaData : IEmailMessageData
    {
        public const string EmailTemplateId = "LykkeCardVisa";

        public string Url { get; set; }
        public int Year { get; set; }
    }
}
