namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class KycRegReminderData : IEmailMessageData
    {
        public const string QueueName = "RegReminderEmail";

        public string Subject { get; set; }
        public string FullName { get; set; }
        public string Year { get; set; }
        public string Date { get; set; }
    }
}
