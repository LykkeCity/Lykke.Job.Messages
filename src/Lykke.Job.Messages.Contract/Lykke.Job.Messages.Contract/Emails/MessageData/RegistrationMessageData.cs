using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class RegistrationMessageData : IEmailMessageData
    {
        public const string QueueName = "WelcomeEmail";

        public string ClientId { get; set; }
        public string Year { get; set; }
    }
}
