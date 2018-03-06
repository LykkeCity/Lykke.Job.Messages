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
using Lykke.Service.Assets.Client.Custom;
using Lykke.Service.EmailPartnerRouter;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.SmsSender.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.TemplateFormatter;
using static Lykke.Job.Messages.Core.AppSettings;
using Lykke.Job.Messages.Utils;
using Lykke.Cqrs;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Contract;
using Lykke.Cqrs.Configuration;
using Lykke.Job.Messages.Contract;

namespace Lykke.Job.Messages.Modules
{
    public class CqrsModule : Module
    {
        public static readonly string Self = EmailMessagesBoundedContext.Name;

        private readonly CqrsSettings _settings;
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly ILog _log;
        private readonly ServiceCollection _services;

        public CqrsModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _appSettings = settings;
            _settings = settings.Nested(x => x.MessagesJob.Cqrs).CurrentValue;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory());

            // Sagas

            // Command handlers

            // Projections

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;

            const string defaultPipeline = "commands";
            const string defaultRoute = "self";

            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    "messagepack",
                    environment: "lykke")),

                Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(defaultRetryDelay));
        }
    }
}
