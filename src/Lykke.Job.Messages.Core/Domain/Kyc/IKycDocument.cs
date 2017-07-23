using System;

namespace Lykke.Job.Messages.Core.Domain.Kyc
{
    public interface IKycDocument
    {
        string ClientId { get; }
        string DocumentId { get; }
        string Type { get; }
        string Mime { get; }
        string KycComment { get; }
        string State { get; }

        string FileName { get; }
        DateTime DateTime { get; }
        string DocumentName { get; }
    }
}