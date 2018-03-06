namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RestrictedAreaData : IEmailMessageData
    {
        public const string EmailTemplateId = "RestrictedAreaEmail";

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
