namespace Lykke.Job.Messages.Core.Domain.Email.MessagesData
{
    public class SwiftConfirmedData : IEmailMessageData
    {
        public string Email { get; set; }
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string AccNumber { get; set; }
        public string AccName { get; set; }
        public string Bic { get; set; }
        public string BlockchainHash { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }

        public string MessageId()
        {
            return "SwiftConfirmedData";
        }
    }
}
