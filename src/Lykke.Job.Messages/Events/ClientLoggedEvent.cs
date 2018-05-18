namespace Lykke.Job.Messages.Events
{
    public class ClientLoggedEvent
    {
        public string ClientId { get; set; }
        public string PartnerId { get; set; }
        public string ClientInfo { get; set; }
        public string UserAgent { get; set; }
        public string Ip { get; set; }
    }
}