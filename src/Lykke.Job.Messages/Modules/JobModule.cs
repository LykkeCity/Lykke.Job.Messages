using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Job.Messages.AzureRepositories.DepositRefId;
using Lykke.Job.Messages.AzureRepositories.Email;
using Lykke.Job.Messages.AzureRepositories.Regulator;
using Lykke.Job.Messages.AzureRepositories.Sms;
using Lykke.Job.Messages.AzureRepositories.SwiftCredentials;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.DepositRefId;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Sms;
using Lykke.Job.Messages.Core.Domain.SwiftCredentials;
using Lykke.Job.Messages.Core.Regulator;
using Lykke.Job.Messages.Core.Services;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Job.Messages.Core.Services.SwiftCredentials;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.QueueConsumers;
using Lykke.Job.Messages.Services;
using Lykke.Job.Messages.Services.Email;
using Lykke.Job.Messages.Services.Http;
using Lykke.Job.Messages.Services.Slack;
using Lykke.Job.Messages.Services.Sms.Mocks;
using Lykke.Job.Messages.Services.SwiftCredentials;
using Lykke.Job.Messages.Services.Templates;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.SmsSender.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.TemplateFormatter;
using BlobSpace = AzureStorage.Blob;

namespace Lykke.Job.Messages.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly IReloadingManager<AppSettings.MessagesSettings> _settings;
        private readonly ILog _log;
        private readonly ServiceCollection _services;
        

        public JobModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _appSettings = settings;
            _settings = settings.Nested(x => x.MessagesJob);
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

            _services.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_appSettings.CurrentValue.Assets.ServiceUrl),
                _settings.CurrentValue.AssetsCache.ExpirationPeriod));
            
            builder.RegisterType<SmsQueueConsumer>().SingleInstance();
            builder.RegisterType<EmailQueueConsumer>().SingleInstance();

            builder.RegisterType<PersonalDataService>()
                .As<IPersonalDataService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PersonalDataServiceSettings));

            builder.RegisterInstance<IClientAccountClient>(
                new Lykke.Service.ClientAccount.Client.ClientAccountClient(_appSettings.CurrentValue
                    .ClientAccountServiceClient.ServiceUrl));

            RegistermSmsServices(builder);
            RegisterEmailServices(builder);
            RegisterSlackServices(builder);
            RegisterRepositories(builder);

            builder.Populate(_services);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.RegisterInstance<IBroadcastMailsRepository>(new BroadcastMailsRepository(
                AzureTableStorage<BroadcastMailEntity>.Create(
                    _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "BroadcastMails", _log)));
            
            builder.RegisterInstance<IRegulatorRepository>(new RegulatorRepository(
                AzureTableStorage<RegulatorEntity>.Create(
                    _settings.ConnectionString(s => s.Db.SharedStorageConnString), "Residences", _log)));

            builder.RegisterInstance<ISmsMockRepository>(new SmsMockRepository(
                AzureTableStorage<SmsMessageMockEntity>.Create(
                    _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "MockSms", _log)));

            builder.RegisterInstance<IDepositRefIdInUseRepository>(new DepositRefIdInUseRepository(
                AzureTableStorage<DepositRefIdInUseEntity>.Create(
                    _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "DepositRefIdsInUse", _log)));

            builder.RegisterInstance<IDepositRefIdRepository>(new DepositRefIdRepository(
                AzureTableStorage<DepositRefIdEntity>.Create(
                    _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "DepositRefIds", _log)));

            builder.RegisterInstance<ISwiftCredentialsRepository>(new SwiftCredentialsRepository(
                AzureTableStorage<SwiftCredentialsEntity>.Create(
                    _settings.ConnectionString(s => s.Db.DictsConnString), "SwiftCredentials", _log)));

            builder.RegisterInstance<ITemplateBlobRepository>(new TemplateBlobRepository(
                BlobSpace.AzureBlobStorage.Create(_settings.ConnectionString(s => s.Db.EmailTemplatesConnString)), "templates"));
        }

        private void RegisterSlackServices(ContainerBuilder builder)
        {
            builder.RegisterType<SrvSlackNotifications>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Slack));
        }

        private void RegisterEmailServices(ContainerBuilder builder)
        {
            var emailsQueue = AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString),
                "emailsqueue");
            var internalEmailQueueReader = new QueueReader(emailsQueue, "InternalEmailQueueReader", 3000, _log);
            var blockChainEmailQueue =
                AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.BitCoinQueueConnectionString), "emailsqueue");
            var blockChainEmailQueueReader =
                new QueueReader(blockChainEmailQueue, "BlockchainEmailQueueReader", 3000, _log);
            var sharedEmailQueue = AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.SharedStorageConnString),
                "emailsqueue");
            var sharedEmailQueueReader = new QueueReader(sharedEmailQueue, "SharedEmailQueueReader", 3000, _log);

            builder.Register<IEnumerable<IQueueReader>>(x => new List<IQueueReader>
                {
                    internalEmailQueueReader,
                    blockChainEmailQueueReader,
                    sharedEmailQueueReader
                })
                .SingleInstance();

            builder.RegisterType<HttpRequestClient>();

            builder.RegisterType<SwiftCredentialsService>()
                .As<ISwiftCredentialsService>()
                .SingleInstance();

            // Email formatting dependencies

            builder.RegisterType<RemoteTemplateGenerator>()
                .As<IRemoteTemplateGenerator>()
                .SingleInstance();
            builder.RegisterTemplateFormatter(_appSettings.CurrentValue.MessagesJob.Email.EmailFormatterUrl, _log);
            builder.RegisterType<EmailGenerator>()
                .As<IEmailGenerator>()
                .SingleInstance()
                .WithParameters(new[]
                {
                    TypedParameter.From(_settings.CurrentValue.Email),
                    TypedParameter.From(_settings.CurrentValue.Blockchain),
                    TypedParameter.From(_settings.CurrentValue.WalletApi)
                });

            builder.RegisterType<EmailTemplateProvider>()
                .As<IEmailTemplateProvide>()
                .SingleInstance();

            //builder.RegisterType<EmailMessageProcessor>()
            //   .As<IEmailMessageProcessor>()
            //   .SingleInstance();

            // Email sending dependencies

            builder.RegisterEmailPartnerRouter(_appSettings.CurrentValue.MessagesJob.Email.EmailPartnerRouterUrl, _log);
            builder.Register<ISmtpEmailSender>(x => new SmtpMailSender(_log, x.Resolve<IEmailPartnerRouter>(), x.Resolve<IBroadcastMailsRepository>()))
                .As<ISmtpEmailSender>()
                .SingleInstance();
        }

        private void RegistermSmsServices(ContainerBuilder builder)
        {
            IQueueExt smsQueue = AzureQueueExt.Create(
                _appSettings.ConnectionString(o => o.SmsNotifications.AzureQueue.ConnectionString),
                _appSettings.CurrentValue.SmsNotifications.AzureQueue.QueueName);

            var smsQueueReader = new QueueReader(smsQueue, "SmsQueueReader", 3000, _log);

            builder.Register<IQueueReader>(x => smsQueueReader).SingleInstance();
            builder.RegisterType<TemplateGenerator>().As<ITemplateGenerator>();

            if (_settings.CurrentValue.Sms.UseMocks)
            {
                builder.RegisterType<SmsMockSender>().As<ISmsSenderClient>().SingleInstance();
            }
            else
            {
                builder.RegisterSmsSenderClient(_appSettings.CurrentValue.SmsSenderServiceClient.ServiceUrl, _log);
            }
        }
    }
}
