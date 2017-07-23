namespace Lykke.Job.Messages.Core.Domain.Email
{
    public enum BroadcastGroup
    {
        Kyc = 100,
        ClientSupport = 200,
        Errors = 300,
        Warnings = 400,
        Payments = 500,
        CompetitionPlatform = 600,
        BtcCashOuts = 700,
        // ReSharper disable once InconsistentNaming
        CFO = 800
    }
}