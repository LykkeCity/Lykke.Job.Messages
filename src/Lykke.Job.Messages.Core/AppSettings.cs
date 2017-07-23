using System;

namespace Lykke.Job.Messages.Core
{
    public class AppSettings
    {
        public MessagesSettings MessagesJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsSettings Assets { get; set; }

        public class MessagesSettings
        {
            public DbSettings Db { get; set; }
            public SmsSettings Sms { get; set; }
            public EmailSettings Email { get; set; }
            public SlackSettings Slack { get; set; }
            public BlockchainSettings Blockchain { get; set; }
            public WalletApiSettings WalletApi { get; set; }
            public AssetsCacheSettings AssetsCache { get; set; }
        }

        public class DbSettings
        {
            public string LogsConnString { get; set; }
            public string ClientPersonalInfoConnString { get; set; }
            public string BitCoinQueueConnectionString { get; set; }
            public string SharedStorageConnString { get; set; }
            public string DictsConnString { get; set; }
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
            public string Host { get; set; }
        }

        public class BlockchainSettings
        {
            public string ExplorerUrl { get; set; }
        }

        public class EmailSettings
        {
            public string EmailTemplatesHost { get; set; }
            public int RefundTimeoutInDays { get; set; }
            public string SmtpHost { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpLogin { get; set; }
            public string SmtpPwd { get; set; }
            public string EmailFromDisplayName { get; set; }
            public string EmailFrom { get; set; }
            public bool UseMocks { get; set; }
        }

        public class SmsSettings
        {
            public TwilioSettings Twilio { get; set; }
            public NexmoSettings Nexmo { get; set; }
            public bool UseMocks { get; set; }
        }

        public class TwilioSettings
        {
            public string AccountSid { get; set; }
            public string AuthToken { get; set; }
            public string SwissSender { get; set; }
            public string UsSender { get; set; }
        }

        public class NexmoSettings
        {
            public string NexmoAppKey { get; set; }
            public string NexmoAppSecret { get; set; }
            public string UsCanadaSender { get; set; }
            public string DefaultSender { get; set; }
        }

        public class AssetsSettings
        {
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
    }
}