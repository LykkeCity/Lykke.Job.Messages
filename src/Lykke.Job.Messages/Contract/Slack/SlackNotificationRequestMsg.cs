namespace Lykke.Job.Messages.Contract.Slack
{
    public class SlackNotificationRequestMsg
    {
        public string Sender { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}