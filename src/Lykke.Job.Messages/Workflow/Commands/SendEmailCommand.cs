using Lykke.Job.Messages.Contract.Emails;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Messages.Workflow.Commands
{
    /// <summary>
    /// Command to send email message
    /// </summary>
    [MessagePackObject]
    public class SendEmailCommand<T> where T : IEmailMessageData
    {
        [Key(0)]
        public string PartnerId { get; set; }

        [Key(1)]
        public string EmailAddress { get; set; }

        [Key(2)]
        public T MessageData { get; set; }
    }
}
