using System;
using Lykke.Job.Messages.Core.Domain.Sms;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.Messages.AzureRepositories.Sms
{
    public class SmsMessageMockEntity : TableEntity, ISmsMockRecord
    {
        public static string GeneratePartitionKey(string phoneNumber)
        {
            return phoneNumber;
        }

        public string Id => RowKey;

        public string PhoneNumber => PartitionKey;

        public DateTime DateTime { get; set; }

        public string From { get; set; }

        public string Text { get; set; }

        public static SmsMessageMockEntity Create(string phoneNumber, SmsMessage smsMessage)
        {
            return new SmsMessageMockEntity
            {
                PartitionKey = GeneratePartitionKey(phoneNumber),
                RowKey = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                Text = smsMessage.Text,
                From = smsMessage.From
            };
        }
    }
}