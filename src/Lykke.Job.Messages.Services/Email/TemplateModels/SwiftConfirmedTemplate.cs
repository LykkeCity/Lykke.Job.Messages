namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class SwiftConfirmedTemplate
    {
        public string Email { get; set; }
        public string AssetId { get; set; }
        public double Amount { get; set; }
        public string AccNumber { get; set; }
        public string AccName { get; set; }
        public string Bic { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }
        public string ExplorerUrl { get; set; }
    }
}
