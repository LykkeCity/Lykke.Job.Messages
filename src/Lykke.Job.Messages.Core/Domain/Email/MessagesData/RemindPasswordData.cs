namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class RemindPasswordData : IEmailMessageData
    {
        public string PasswordHint { get; set; }
        public string MessageId()
        {
            return "RemindPasswordEmail";
        }

        public static RemindPasswordData Create(string passwordHint)
        {
            return new RemindPasswordData
            {
                PasswordHint = passwordHint
            };
        }

    }
}
