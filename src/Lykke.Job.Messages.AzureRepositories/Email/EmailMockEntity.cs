using System;
using Lykke.Job.Messages.Core.Domain.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class EmailMockEntity : TableEntity, ISmtpMailMock
    {
        public static string GeneratePartitionKey(string email)
        {
            return email.ToLower();
        }

        public string Id => RowKey;

        public string Address => PartitionKey;

        public DateTime DateTime { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public bool IsHtml { get; set; }

        public static EmailMockEntity Create(string address, EmailMessage emailMessage)
        {
            return new EmailMockEntity
            {
                PartitionKey = GeneratePartitionKey(address),
                RowKey = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsHtml = emailMessage.IsHtml
            };
        }
    }
}