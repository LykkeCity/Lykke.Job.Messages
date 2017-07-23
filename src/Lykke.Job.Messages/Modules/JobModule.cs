using System;
using System.Collections.Generic;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Job.Messages.AzureRepositories.Clients;
using Lykke.Job.Messages.AzureRepositories.Email;
using Lykke.Job.Messages.AzureRepositories.Regulator;
using Lykke.Job.Messages.AzureRepositories.Sms;
using Lykke.Job.Messages.AzureRepositories.SwiftCredentials;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.Clients;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Job.Messages.Core.Regulator;
using Lykke.Job.Messages.Core.Services;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Core.Services.Sms;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.QueueConsumers;
using Lykke.Job.Messages.Services;
using Lykke.Job.Messages.Services.Email;
using Lykke.Job.Messages.Services.Http;
using Lykke.Job.Messages.Services.Slack;
using Lykke.Job.Messages.Services.Sms;
using Lykke.Job.Messages.Services.Sms.Mocks;
using Lykke.Job.Messages.Services.Sms.Nexmo;
using Lykke.Job.Messages.Services.Sms.Twilio;
using Lykke.Job.Messages.Services.SwiftCredentials;
using Lykke.Job.Messages.Services.Templates;
using Lykke.Service.Assets.Client.Custom;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace Lykke.Job.Messages.Modules
{
    public class JobModule : Module
    {
        private readonly AppSettings _appSettings;
        private readonly AppSettings.MessagesSettings _settings;
        private readonly ILog _log;
        private readonly ServiceCollection _services;
        

        public JobModule(AppSettings settings, ILog log)
        {
            _appSettings = settings;
            _settings = settings.MessagesJob;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            // NOTE: You can implement your own poison queue notifier. See https://github.com/LykkeCity/JobTriggers/blob/master/readme.md
            // builder.Register<PoisionQueueNotifierImplementation>().As<IPoisionQueueNotifier>();

            _services.UseAssetsClient(new AssetServiceSettings
            {
                BaseUri = new Uri(_appSettings.Assets.ServiceUrl),
                AssetPairsCacheExpirationPeriod = _settings.AssetsCache.ExpirationPeriod,
                AssetsCacheExpirationPeriod = _settings.AssetsCache.ExpirationPeriod
            });

            builder.RegisterType<SmsQueueConsumer>().SingleInstance();
            builder.RegisterType<EmailQueueConsumer>().SingleInstance();

            RegistermSmsServices(builder);
            RegisterEmailServices(builder);
            RegisterSlackServices(builder);

            RegisterRepositories(builder);

            builder.Populate(_services);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IPersonalDataRepository>(
                new PersonalDataRepository(
                    new AzureTableStorage<PersonalDataEntity>(_settings.Db.ClientPersonalInfoConnString, "PersonalData", _log)));

            builder.RegisterInstance<IAttachmentFileRepository>(
                new AttachmentFileRepository(
                    new AzureBlobStorage(_settings.Db.ClientPersonalInfoConnString)));

            builder.RegisterInstance<IBroadcastMailsRepository>(
                new BroadcastMailsRepository(
                    new AzureTableStorage<BroadcastMailEntity>(_settings.Db.ClientPersonalInfoConnString, "BroadcastMails", _log)));

            builder.RegisterInstance<IEmailAttachmentsMockRepository>(
                new EmailAttachmentsMockRepository(
                    new AzureTableStorage<EmailAttachmentsMockEntity>(_settings.Db.ClientPersonalInfoConnString, "EmailAttachmentsMock", _log)));

            builder.RegisterInstance<IEmailMockRepository>(
                new EmailMockRepository(
                    new AzureTableStorage<EmailMockEntity>(_settings.Db.ClientPersonalInfoConnString, "MockMails", _log)));

            builder.RegisterInstance<IRegulatorRepository>(
                new RegulatorRepository(
                    new AzureTableStorage<RegulatorEntity>(_settings.Db.SharedStorageConnString, "Residences", _log)));

            builder.RegisterInstance<ISmsMockRepository>(
                new SmsMockRepository(
                    new AzureTableStorage<SmsMessageMockEntity>(_settings.Db.ClientPersonalInfoConnString, "MockSms", _log)));

            builder.RegisterInstance<ISwiftCredentialsRepository>(
                new SwiftCredentialsRepository(
                    new AzureTableStorage<SwiftCredentialsEntity>(_settings.Db.DictsConnString, "SwiftCredentials", _log)));
        }

        private void RegisterSlackServices(ContainerBuilder builder)
        {
            builder.RegisterType<SrvSlackNotifications>()
                .WithParameter(TypedParameter.From(_settings.Slack));
        }

        private void RegisterEmailServices(ContainerBuilder builder)
        {
            var emailsQueue = new AzureQueueExt(_settings.Db.ClientPersonalInfoConnString, "emailsqueue");
            var internalEmailQueueReader = new QueueReader(emailsQueue, "InternalEmailQueueReader", 3000, _log);
            var blockChainEmailQueue = new AzureQueueExt(_settings.Db.BitCoinQueueConnectionString, "emailsqueue");
            var blockChainEmailQueueReader = new QueueReader(blockChainEmailQueue, "BlockchainEmailQueueReader", 3000, _log);
            var sharedEmailQueue = new AzureQueueExt(_settings.Db.SharedStorageConnString, "emailsqueue");
            var sharedEmailQueueReader = new QueueReader(sharedEmailQueue, "SharedEmailQueueReader", 3000, _log);

            builder.Register<IEnumerable<IQueueReader>>(x => new List<IQueueReader>
                {
                    internalEmailQueueReader,
                    blockChainEmailQueueReader,
                    sharedEmailQueueReader
                })
                .SingleInstance();
            builder.RegisterType<EmailGenerator>()
                .As<IEmailGenerator>()
                .SingleInstance()
                .WithParameters(new []
                {
                    TypedParameter.From(_settings.Email),
                    TypedParameter.From(_settings.Blockchain),
                    TypedParameter.From(_settings.WalletApi)
                });
            builder.RegisterType<HttpRequestClient>();
            builder.RegisterType<RemoteTemplateGenerator>()
                .As<IRemoteTemplateGenerator>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.Email.EmailTemplatesHost));
            builder.RegisterType<SwiftCredentialsService>().As<ISwiftCredentialsService>().SingleInstance();

            SmtpClient ClientFactory()
            {
                var client = new SmtpClient
                {
                    Timeout = 10000
                };

                client.Connect(_settings.Email.SmtpHost, _settings.Email.SmtpPort);
                client.Authenticate(new NetworkCredential(_settings.Email.SmtpLogin, _settings.Email.SmtpPwd));

                return client;
            }

            var from = new MailboxAddress(_settings.Email.EmailFromDisplayName, _settings.Email.EmailFrom);

            if (_settings.Email.UseMocks)
            {
                builder.RegisterType<SmtpMailSenderMock>().SingleInstance();
                builder.Register(x => new SmtpMailSender(_log, ClientFactory, from, x.Resolve<IBroadcastMailsRepository>()));
                builder.RegisterType<MockAndRealMailSender>().As<ISmtpEmailSender>().SingleInstance();
            }
            else
            {
                builder.Register<ISmtpEmailSender>(x => new SmtpMailSender(_log, ClientFactory, from, x.Resolve<IBroadcastMailsRepository>()));
            }
        }

        private void RegistermSmsServices(ContainerBuilder builder)
        {
            var smsQueue = new AzureQueueExt(_settings.Db.ClientPersonalInfoConnString, "smsqueue");
            var smsQueueReader = new QueueReader(smsQueue, "SmsQueueReader", 3000, _log);

            builder.Register<IQueueReader>(x => smsQueueReader).SingleInstance();
            builder.RegisterType<TemplateGenerator>().As<ITemplateGenerator>();
            builder.RegisterType<SmsTextGenerator>().As<ISmsTextGenerator>().SingleInstance();

            if (_settings.Sms.UseMocks)
            {
                builder.RegisterType<SmsMockSender>().As<ISmsSender>().SingleInstance();
                builder.RegisterType<AlternativeSmsMockSender>().As<IAlternativeSmsSender>().SingleInstance();
            }
            else
            {
                builder.RegisterType<NexmoSmsSender>().As<ISmsSender>().SingleInstance();
                builder.RegisterType<TwilioSmsSender>().As<IAlternativeSmsSender>().SingleInstance();
            }
        }
    }
}