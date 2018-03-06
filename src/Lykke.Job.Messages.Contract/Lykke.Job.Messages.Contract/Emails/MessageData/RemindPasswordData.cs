namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RemindPasswordData : IEmailMessageData
    {
        public const string QueueName = "RemindPasswordEmail";

        public string PasswordHint { get; set; }
    }
}
