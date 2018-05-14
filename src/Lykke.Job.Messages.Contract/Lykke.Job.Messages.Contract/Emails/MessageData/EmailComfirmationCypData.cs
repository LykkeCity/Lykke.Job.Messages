namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class EmailComfirmationCypData : IEmailMessageData
    {
        public const string EmailTemplateId = "EmailConfirmationCypTemplate";

        public string ConfirmationCode { get; set; }
        public string Year { get; set; }
    }
}