namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RegistrationCypMessageData : IEmailMessageData
    {
        public const string EmailTemplateId = "WelcomeCypEmail";

        public string ClientId { get; set; }
        public string Year { get; set; }
    }
}
