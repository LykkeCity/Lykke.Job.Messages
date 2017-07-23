namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class BankCashInTemplate
    {
        public string AssetId { get; set; }
        public string AssetSymbol { get; set; }
        public string ClientName { get; set; }
        public string Bic { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PurposeOfPayment { get; set; }
        public string BankAddress { get; set; }
        public string CompanyAddress { get; set; }
        public double Amount { get; set; }
        public string Year { get; set; }
    }
}