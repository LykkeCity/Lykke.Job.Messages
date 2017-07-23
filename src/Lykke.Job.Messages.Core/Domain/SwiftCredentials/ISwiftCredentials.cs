namespace Lykke.Job.Messages.Core.Domain.SwiftCredentials
{
    public interface ISwiftCredentials
    {
        string RegulatorId { get; }
        string AssetId { get; }
        string BIC { get; }
        string AccountNumber { get; }
        string AccountName { get; }
        string PurposeOfPayment { get; }
        string BankAddress { get; }
        string CompanyAddress { get; }
    }
}