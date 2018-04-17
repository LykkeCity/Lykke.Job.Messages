namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class KycOkData : IEmailMessageData
    {
        public const string EmailTemplateId = "WelcomeFxEmail";

        public string ClientId { get; set; }
        public string Year { get; set; }
    }
}
