using System;

namespace Lykke.Job.Messages.Core.Domain.Email
{
    public interface ISmtpMailMock
    {
        string Id { get; }

        string Address { get; }

        DateTime DateTime { get; }

        string Subject { get; }

        string Body { get; }

        bool IsHtml { get; }
    }
}