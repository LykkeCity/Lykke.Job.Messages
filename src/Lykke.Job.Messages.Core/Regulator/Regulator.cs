namespace Lykke.Job.Messages.Core.Regulator
{
    public class Regulator : IRegulator
    {
        public string InternalId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Countries { get; set; }
        public string TermsOfUseUrl { get; set; }
        public string CreditVoucherUrl { get; set; }
        public string MarginTradingConditions { get; set; }
        public string RiskDescriptionUrl { get; set; }
    }
}