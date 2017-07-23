namespace Lykke.Job.Messages.Core.Regulator
{
    public interface IRegulator
    {
        string Name { get; }
        string InternalId { get; }
        bool IsDefault { get; }
        string Countries { get; }
        string CreditVoucherUrl { get; }
        string MarginTradingConditions { get; }
        string TermsOfUseUrl { get; set; }
        string RiskDescriptionUrl { get; set; }
    }
}