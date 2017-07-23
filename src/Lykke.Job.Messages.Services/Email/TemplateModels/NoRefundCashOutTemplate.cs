namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class NoRefundCashOutTemplate
    {
        public string AssetId { get; set; }
        public double? Amount { get; set; }
        public string ExplorerUrl { get; set; }
        public int Year { get; set; }
    }
}
