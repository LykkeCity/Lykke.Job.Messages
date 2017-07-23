using Lykke.Job.Messages.Core.Regulator;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Regulator
{
    public class RegulatorEntity : TableEntity, IRegulator
    {
        public string InternalId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Countries { get; set; }
        public string TermsOfUseUrl { get; set; }
        public string CreditVoucherUrl { get; set; }
        public string MarginTradingConditions { get; set; }
        public string RiskDescriptionUrl { get; set; }

        public static string GeneratePartition()
        {
            return "Regulator";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static RegulatorEntity Create(IRegulator regulator)
        {
            return new RegulatorEntity
            {
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(regulator.InternalId),
                InternalId = regulator.InternalId,
                Name = regulator.Name,
                IsDefault = regulator.IsDefault,
                Countries = regulator.Countries,
                TermsOfUseUrl = regulator.TermsOfUseUrl,
                MarginTradingConditions = regulator.MarginTradingConditions,
                CreditVoucherUrl = regulator.CreditVoucherUrl,
                RiskDescriptionUrl = regulator.RiskDescriptionUrl
            };
        }

        public static RegulatorEntity Update(RegulatorEntity from, IRegulator to)
        {
            from.Name = to.Name;
            from.InternalId = to.InternalId;
            from.IsDefault = to.IsDefault;
            from.Countries = to.Countries;
            from.TermsOfUseUrl = to.TermsOfUseUrl;
            from.MarginTradingConditions = to.MarginTradingConditions;
            from.CreditVoucherUrl = to.CreditVoucherUrl;
            from.RiskDescriptionUrl = to.RiskDescriptionUrl;
            return Create(from);
        }
    }
}