namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RegistrationEmailVerifyData : IEmailMessageData
    {
        public const string EmailTemplateId = "RegistrationVerifyEmail";

        public string Code { get; set; }
        public string Url { get; set; }
        public string Year { get; set; }
    }
}
