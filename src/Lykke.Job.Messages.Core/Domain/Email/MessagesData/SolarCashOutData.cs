namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class SolarCashOutData : IEmailMessageData
    {
        public string AddressTo { get; set; }
        public double Amount { get; set; }

        public string MessageId()
        {
            return "SolarCashOut";
        }
    }
}
