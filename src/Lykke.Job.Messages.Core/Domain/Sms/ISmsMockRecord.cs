using System;

namespace Lykke.Job.Messages.Core.Domain.Sms
{
    public interface ISmsMockRecord
    {
        string Id { get; }
        string PhoneNumber { get; }
        DateTime DateTime { get; }
        string From { get; }
        string Text { get; }
    }
}