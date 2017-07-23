namespace Lykke.Job.Messages.Services.Email.TemplateModels
{
    public class TransferTemplate
    {
        public string ClientName { get; set; }
        public double AmountFiat { get; set; }
        public double AmountLkk { get; set; }
        public string Price { get; set; }
        public string AssetId { get; set; }
        public string ExplorerUrl { get; set; }
        public int Year { get; set; }
    }
}
