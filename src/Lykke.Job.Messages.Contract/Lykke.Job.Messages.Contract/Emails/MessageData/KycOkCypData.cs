namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class KycOkCypData : IEmailMessageData
    {
        public const string EmailTemplateId = "WelcomeFxCypEmail";

        public string ClientId { get; set; }
        public string Year { get; set; }
    }
}
