using System;

namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class FreezePeriodNotificationData : IEmailMessageData
    {
        public const string EmailTemplateId = "FreezePeriodNotificationEmail";

        public DateTime FreezePeriod { get; set; }
        public string Year { get; set; }
    }
}