namespace Lykke.Job.Messages.Contract.Emails.MessageData
{
    public class SolarCashOutData : IEmailMessageData
    {
        public const string EmailTemplateId = "SolarCashOut";

        public string AddressTo { get; set; }
        public double Amount { get; set; }
    }
}
