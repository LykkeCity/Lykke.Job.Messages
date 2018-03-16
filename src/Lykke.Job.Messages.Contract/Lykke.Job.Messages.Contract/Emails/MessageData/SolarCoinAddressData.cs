namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SolarCoinAddressData : IEmailMessageData
    {
        public const string EmailTemplateId = "SolarCoinAddress";

        public string Address { get; set; }
    }
}
