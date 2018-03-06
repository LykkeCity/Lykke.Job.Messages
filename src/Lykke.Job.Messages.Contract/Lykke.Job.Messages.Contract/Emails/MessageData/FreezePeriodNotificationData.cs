using System;

namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class FreezePeriodNotificationData : IEmailMessageData
    {
        public const string QueueName = "FreezePeriodNotificationEmail";

        public DateTime FreezePeriod { get; set; }
        public string Year { get; set; }
    }
}