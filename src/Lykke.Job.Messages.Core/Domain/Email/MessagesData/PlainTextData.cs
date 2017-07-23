namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class PlainTextData : IEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }

        public string MessageId()
        {
            return "PlainTextEmail";
        }

        public static PlainTextData Create(string subject, string text, string sender = null)
        {
            return new PlainTextData
            {
                Subject = subject,
                Sender = sender,
                Text = text
            };
        }
    }


    public class PlainTextBroadCastData : IEmailMessageData
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }

        public string MessageId()
        {
            return "PlainTextBroadcast";
        }

        public static PlainTextBroadCastData Create(string subject, string text, string sender = null)
        {
            return new PlainTextBroadCastData
            {
                Subject = subject,
                Sender = sender,
                Text = text
            };
        }
    }
}

