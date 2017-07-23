namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class SolarCoinAddressData : IEmailMessageData
    {
        public string Address { get; set; }

        public string MessageId()
        {
            return "SolarCoinAddress";
        }
    }
}
