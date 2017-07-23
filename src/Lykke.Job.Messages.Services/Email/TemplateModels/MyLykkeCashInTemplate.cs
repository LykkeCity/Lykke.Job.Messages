namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class MyLykkeCashInTemplate
    {
        public string Year { get; set; }
        public string ConversionWalletAddress { get; set; }
        public double Amount { get; set; }
        public double LkkAmount { get; set; }
        public double Price { get; set; }
        public string AssetId { get; set; }
    }
}
