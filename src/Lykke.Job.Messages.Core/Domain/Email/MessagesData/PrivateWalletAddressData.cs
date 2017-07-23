namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class PrivateWalletAddressData : IEmailMessageData
    {
        public string Address { get; set; }
        public string Name { get; set; }

        public string MessageId()
        {
            return "PrivateWalletAddressEmail";
        }
    }
}
