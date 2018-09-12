using System;
using System.Collections.Generic;
using Autofac;
using AzureStorage.Queue;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.Messages.AzureRepositories.Deduplication;
using Lykke.Job.Messages.AzureRepositories.DepositRefId;
using Lykke.Job.Messages.AzureRepositories.Email;
using Lykke.Job.Messages.AzureRepositories.Regulator;
using Lykke.Job.Messages.AzureRepositories.Sms;
using Lykke.Job.Messages.AzureRepositories.SwiftCredentials;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Core.Domain.Deduplication;
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
using Lykke.JobTriggers.Extenstions;
using Lykke.Logs;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.EmailPartnerRouter;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.SmsSender.Client;
using Lykke.SettingsReader;
using Lykke.Service.TemplateFormatter;
using BlobSpace = AzureStorage.Blob;

namespace Lykke.Job.Messages.Modules
{
    [UsedImplicitly]
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly IReloadingManager<AppSettings.MessagesSettings> _settings;

        public JobModule(IReloadingManager<AppSettings> settings)
        {
            _appSettings = settings;
            _settings = settings.Nested(x => x.MessagesJob);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();
            
            builder.RegisterAssetsClient(AssetServiceSettings.Create(
                new Uri(_appSettings.CurrentValue.Assets.ServiceUrl),
                _settings.CurrentValue.AssetsCache.ExpirationPeriod));

            builder.RegisterType<SmsQueueConsumer>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder.RegisterType<EmailQueueConsumer>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.Register(ctx =>
                    new PersonalDataService(_appSettings.CurrentValue.PersonalDataServiceSettings,
                        ctx.Resolve<ILogFactory>().CreateLog(nameof(PersonalDataService))))
                .As<IPersonalDataService>()
                .SingleInstance();

            builder.RegisterInstance<IClientAccountClient>(
                new ClientAccountClient(_appSettings.CurrentValue
                    .ClientAccountServiceClient.ServiceUrl));

            builder.RegisterInstance(new PayInvoiceClient(_appSettings.CurrentValue.PayInvoiceServiceClient))
                .As<IPayInvoiceClient>()
                .SingleInstance();

            RegistermSmsServices(builder);
            RegisterEmailServices(builder);
            RegisterSlackServices(builder);
            RegisterRepositories(builder);

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(_settings.Nested(x => x.Db.SharedStorageConnString));
            });
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            builder.Register(ctx => new BroadcastMailsRepository(
                    AzureTableStorage<BroadcastMailEntity>.Create(
                        _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "BroadcastMails",
                        ctx.Resolve<ILogFactory>()))
                ).As<IBroadcastMailsRepository>()
                .SingleInstance();

            builder.Register(ctx => new RegulatorRepository(
                    AzureTableStorage<RegulatorEntity>.Create(
                        _settings.ConnectionString(s => s.Db.SharedStorageConnString), "Residences",
                        ctx.Resolve<ILogFactory>())))
                .As<IRegulatorRepository>()
                .SingleInstance();

            builder.Register(ctx => new SmsMockRepository(
                    AzureTableStorage<SmsMessageMockEntity>.Create(
                        _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "MockSms",
                        ctx.Resolve<ILogFactory>())))
                .As<ISmsMockRepository>()
                .SingleInstance();

            builder.Register(ctx => new DepositRefIdInUseRepository(
                AzureTableStorage<DepositRefIdInUseEntity>.Create(
                    _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "DepositRefIdsInUse", ctx.Resolve<ILogFactory>())))
                .As<IDepositRefIdInUseRepository>()
                .SingleInstance();

            builder.Register(ctx => new DepositRefIdRepository(
                    AzureTableStorage<DepositRefIdEntity>.Create(
                        _settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "DepositRefIds",
                        ctx.Resolve<ILogFactory>())))
                .As<IDepositRefIdRepository>()
                .SingleInstance();

            builder.Register(ctx => new SwiftCredentialsRepository(
                    AzureTableStorage<SwiftCredentialsEntity>.Create(
                        _settings.ConnectionString(s => s.Db.DictsConnString), "SwiftCredentials",
                        ctx.Resolve<ILogFactory>())))
                .As<ISwiftCredentialsRepository>()
                .SingleInstance();

            builder.RegisterInstance<ITemplateBlobRepository>(new TemplateBlobRepository(
                BlobSpace.AzureBlobStorage.Create(_settings.ConnectionString(s => s.Db.EmailTemplatesConnString)), "templates"));

            builder.Register(ctx => DeduplicationRepository.Create(
                    _settings.ConnectionString(x => x.Db.SharedStorageConnString),
                    ctx.Resolve<ILogFactory>()))
                .As<IOperationMessagesDeduplicationRepository>()
                .SingleInstance();
        }

        private void RegisterSlackServices(ContainerBuilder builder)
        {
            builder.RegisterType<SrvSlackNotifications>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Slack));
        }

        private void RegisterEmailServices(ContainerBuilder builder)
        {
            var emailsQueue = AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.ClientPersonalInfoConnString), "emailsqueue");
            var blockChainEmailQueue = AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.BitCoinQueueConnectionString), "emailsqueue");
            var sharedEmailQueue = AzureQueueExt.Create(_settings.ConnectionString(s => s.Db.SharedStorageConnString), "emailsqueue");

            builder.Register<IEnumerable<IQueueReader>>(ctx => new List<IQueueReader>
                {
                    new QueueReader(emailsQueue, "InternalEmailQueueReader", TimeSpan.FromMilliseconds(3000), ctx.Resolve<ILogFactory>()),
                    new QueueReader(blockChainEmailQueue, "BlockchainEmailQueueReader", TimeSpan.FromMilliseconds(3000), ctx.Resolve<ILogFactory>()),
                    new QueueReader(sharedEmailQueue, "SharedEmailQueueReader", TimeSpan.FromMilliseconds(3000), ctx.Resolve<ILogFactory>())
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

            builder.RegisterTemplateFormatter(_appSettings.CurrentValue.MessagesJob.Email.EmailFormatterUrl);
            
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

            // Email sending dependencies
            builder.Register(ctx => 
                new EmailPartnerRouterClient(_appSettings.CurrentValue.MessagesJob.Email.EmailPartnerRouterUrl, 
                    ctx.Resolve<ILogFactory>().CreateLog(nameof(EmailPartnerRouterClient))))
                .As<IEmailPartnerRouter>().SingleInstance();
            
            builder.Register<ISmtpEmailSender>(ctx => 
                    new SmtpMailSender(ctx.Resolve<ILogFactory>(), ctx.Resolve<IEmailPartnerRouter>(), ctx.Resolve<IBroadcastMailsRepository>()))
                .As<ISmtpEmailSender>()
                .SingleInstance();
        }

        private void RegistermSmsServices(ContainerBuilder builder)
        {
            IQueueExt smsQueue = AzureQueueExt.Create(
                _appSettings.ConnectionString(o => o.SmsNotifications.AzureQueue.ConnectionString),
                _appSettings.CurrentValue.SmsNotifications.AzureQueue.QueueName);

            builder.Register<IQueueReader>(ctx => 
                new QueueReader(smsQueue, "SmsQueueReader", TimeSpan.FromMilliseconds(3000), ctx.Resolve<ILogFactory>())
                ).SingleInstance();
            
            builder.RegisterType<TemplateGenerator>().As<ITemplateGenerator>();

            if (_settings.CurrentValue.Sms.UseMocks)
            {
                builder.RegisterType<SmsMockSender>().As<ISmsSenderClient>().SingleInstance();
            }
            else
            {
                builder.Register(ctx =>
                        new SmsSenderClient(_appSettings.CurrentValue.SmsSenderServiceClient.ServiceUrl,
                            ctx.Resolve<ILogFactory>().CreateLog(nameof(SmsSenderClient))))
                    .As<ISmsSenderClient>()
                    .SingleInstance();
            }
        }
    }
}
