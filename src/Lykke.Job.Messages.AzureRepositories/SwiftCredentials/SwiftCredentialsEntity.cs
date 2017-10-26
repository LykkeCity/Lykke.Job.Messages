using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.SwiftCredentials
{
    public class SwiftCredentialsEntity : TableEntity, ISwiftCredentials
    {
        public string RegulatorId { get; set; }
        public string AssetId { get; set; }
        public string BIC { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PurposeOfPayment { get; set; }
        public string BankAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CorrespondentAccount { get; set; }

        public static string GeneratePartitionKey(string regulatorId)
        {
            return regulatorId;
        }

        public static string GenerateRowKey(string assetId)
        {
            return assetId ?? "*";
        }

        public static SwiftCredentialsEntity Create(ISwiftCredentials credentials)
        {
            return new SwiftCredentialsEntity
            {
                PartitionKey = GeneratePartitionKey(credentials.RegulatorId),
                RowKey = GenerateRowKey(credentials.AssetId),
                RegulatorId = credentials.RegulatorId,
                AssetId = credentials.AssetId,
                AccountName = credentials.AccountName,
                AccountNumber = credentials.AccountNumber,
                BIC = credentials.BIC,
                BankAddress = credentials.BankAddress,
                CompanyAddress = credentials.CompanyAddress,
                PurposeOfPayment = credentials.PurposeOfPayment,
                CorrespondentAccount = credentials.CorrespondentAccount
            };
        }
    }
}