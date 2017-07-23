namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class RegistrationData : IEmailMessageData
    {
        public string ClientId { get; set; }
        public string Year { get; set; }
        public string MessageId()
        {
            return "WelcomeEmail";
        }
    }
}
