﻿using System;
using Lykke.Job.Messages.Core;
using Lykke.Sdk.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PersonalData.Settings;
using Lykke.Service.SwiftCredentials.Client;
using Lykke.Service.TemplateFormatter.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Messages
{
    public class AppSettings : BaseAppSettings
    {
        public MessagesSettings MessagesJob { get; set; }
        public AssetsSettings Assets { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceSettings { get; set; }
        public KycServiceClientSettings KycServiceSettings { get; set; }
        public LykkeKycWebsiteUrlSettings LykkeKycWebsiteUrlSettings { get; set; }
        public SmsNotificationsSettings SmsNotifications { get; set; }
        public SmsSenderSettings SmsSenderServiceClient { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
        public SagasRabbitMq SagasRabbitMq { get; set; }
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        public TemplateFormatterServiceClientSettings TemplateFormatterServiceClient { get; set; }
        public SwiftCredentialsServiceClientSettings SwiftCredentialsServiceClient { get; set; }

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
        }

        public class DbSettings
        {
            public string LogsConnString { get; set; }
            public string ClientPersonalInfoConnString { get; set; }
            public string EmailTemplatesConnString { get; set; }
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
            public SlackChannel[] Channels { get; set; }
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

        public class AzureQueueSettings
        {
            public string ConnectionString { get; set; }

            public string QueueName { get; set; }
        }

        public class SmsNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }
        }       
    }

    public class LykkeKycWebsiteUrlSettings
    {
        public string Url { get; set; }
    }

    public class SagasRabbitMq
    {
        [AmqpCheck]
        public string RabbitConnectionString { get; set; }

        public string RetryDelay { get; set; }
    }

    public class Transports
    {
        public string ClientRabbitMqConnectionString { get; set; }
    }
}
