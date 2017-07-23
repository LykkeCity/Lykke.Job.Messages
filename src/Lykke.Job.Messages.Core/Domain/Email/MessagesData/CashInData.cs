namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class CashInData : IEmailMessageData
    {
        public string Multisig { get; set; }
        public string AssetId { get; set; }

        public string MessageId()
        {
            return "CashInEmail";
        }
    }
}
