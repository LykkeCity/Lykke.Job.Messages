namespace Lykke.Job.Messages.Contract.Emails.MessageData
{    
    public class PasswordRecoveryEmailComfirmationData : IEmailMessageData
    {
        public const string QueueName = "PasswordRecoveryConfirmationEmail";

        public string ConfirmationCode { get; set; }
        public string Year { get; set; }
    }    
}