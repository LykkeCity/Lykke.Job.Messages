using System;
using JetBrains.Annotations;
using Lykke.Service.PersonalData.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Messages.Core
{
    public class AppSettings
    {
        public MessagesSettings MessagesJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsSettings Assets { get; set; }
        public PersonalDataServiceSettings PersonalDataServiceSettings { get; set; }
        public SmsNotificationsSettings SmsNotifications { get; set; }
        public SmsSenderSettings SmsSenderServiceClient { get; set; }

        public class MessagesSettings
        {
            public Transports Transports { get; set; }
            public DbSettings Db { get; set; }
            public SmsSettings Sms { get; set; }
            public EmailSettings Email { get; set; }
            public SlackSettings Slack { get; set; }
            public BlockchainSettings Blockchain { get; set; }
            public WalletApiSettings WalletApi { get; set; }
            public AssetsCacheSettings AssetsCache { get; set; }
            public CqrsSettings Cqrs { get; set; }
            public int EmailRetryPeriodInMinutes { get; set; }
        }

        public class DbSettings
        {
            public string LogsConnString { get; set; }
            public string ClientPersonalInfoConnString { get; set; }
            public string EmailTemplatesConnString { get; set; }
            public string BitCoinQueueConnectionString { get; set; }
            public string SharedStorageConnString { get; set; }
            public string DictsConnString { get; set; }
            public string PartnerEmailTemplatesConnectionString { get; set; }
        }

        public class AssetsCacheSettings
        {
            public TimeSpan ExpirationPeriod { get; set; }
        }

        public class SlackSettings
        {
            public class Channel
            {
                public string Type { get; set; }
                public string WebHookUrl { get; set; }
            }

            public string Env { get; set; }
            public Channel[] Channels { get; set; }
        }

        public class WalletApiSettings
        {
            [HttpCheck("/api/isalive")]
            public string Host { get; set; }
        }

        public class BlockchainSettings
        {
            public string ExplorerUrl { get; set; }
        }

        public class EmailSettings
        {
            [HttpCheck("/api/isalive")]
            public string EmailFormatterUrl { get; set; }
            [HttpCheck("/api/isalive")]
            public string EmailPartnerRouterUrl { get; set; }
            public int RefundTimeoutInDays { get; set; }
        }

        public class SmsSettings
        {
            public bool UseMocks { get; set; }
        }
        
        public class SmsSenderSettings
        {
            [HttpCheck("/api/isalive")]
            public string ServiceUrl { get; set; }
        }

        public class AssetsSettings
        {
            [HttpCheck("/api/isalive")]
            public string ServiceUrl { get; set; }
        }

        public class SlackNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }

            public int ThrottlingLimitSeconds { get; set; }
        }

        public class AzureQueueSettings
        {
            public string ConnectionString { get; set; }

            public string QueueName { get; set; }
        }

        public class SmsNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }
        }

        public class CqrsSettings
        {
            [AmqpCheck]
            [UsedImplicitly(ImplicitUseKindFlags.Assign)]
            public string RabbitConnectionString { get; set; }

            [UsedImplicitly(ImplicitUseKindFlags.Assign)]
            public TimeSpan RetryDelay { get; set; }
        }
    }

    public class Transports
    {
        public string ClientRabbitMqConnectionString { get; set; }
    }
}
