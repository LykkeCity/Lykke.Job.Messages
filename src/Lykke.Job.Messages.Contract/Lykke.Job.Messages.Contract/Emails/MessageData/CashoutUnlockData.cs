namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class CashoutUnlockData: IEmailMessageData
    {
        public const string EmailTemplateId = "CashoutUnlockEmail";

        public string Code { get; set; }
        public string ClientId { get; set; }
    }
}
