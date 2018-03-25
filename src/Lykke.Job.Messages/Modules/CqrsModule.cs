using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Job.Messages.Core;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using static Lykke.Job.Messages.Core.AppSettings;
using Lykke.Job.Messages.Utils;
using Lykke.Cqrs;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Cqrs.Configuration;
using Lykke.Job.Messages.Contract;
using System;
using System.Linq;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Handlers;
using Lykke.Job.Messages.Sagas;
using Lykke.Job.Messages.Workflow;
using Lykke.Service.PushNotifications.Contract;

namespace Lykke.Job.Messages.Modules
{
    public class CqrsModule : Module
    {
        public static readonly string Self = EmailMessagesBoundedContext.Name;

        private readonly IReloadingManager<AppSettings.MessagesSettings> _settings;
        private readonly ILog _log;

        public CqrsModule(IReloadingManager<AppSettings.MessagesSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string selfRoute = "self";
            string commandsRoute = "commands";
            string eventsRoute = "events";
            Messaging.Serialization.MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.Transports.ClientRabbitMqConnectionString };
            var rabbitMqMeSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.Cqrs.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();

            builder.RegisterType<BlockchainOperationsSaga>().SingleInstance();
            builder.RegisterType<LoginNotificationsSaga>().SingleInstance();
            builder.RegisterType<EmailNotificationsCommandHandler>().WithParameter(TypedParameter.From(_settings.CurrentValue.EmailRetryPeriodInMinutes)).SingleInstance();

            #region RegistrationRabbit

            var messagingEngineRegistrationRabbit = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory());

            builder.Register(ctx =>
            {
                return new CqrsEngine(_log,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngineRegistrationRabbit,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        "messagepack",
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),

                    Register.Saga<LoginNotificationsSaga>("login-notifications-saga")
                        .ListeningEvents(typeof(ClientLoggedEvent))
                        .From("registration").On(eventsRoute)
                        .PublishingCommands(typeof(SendEmailCommand)).To(Self).With(commandsRoute)
                        .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                    Register.BoundedContext(Self)
                        .ListeningCommands(typeof(SendEmailCommand))
                        .On(commandsRoute)
                        .WithLoopback()
                        .WithCommandsHandler<EmailNotificationsCommandHandler>()
                        .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256)
                    );
            })
            .Keyed<ICqrsEngine>(RabbitType.Registration)
            .SingleInstance()
            .AutoActivate();

            #endregion

            #region ME_Rabbit

            var messagingEngineMeRabbit = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqMeSettings.Endpoint.ToString(), rabbitMqMeSettings.UserName, rabbitMqMeSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory());

            var pushNotificationsCommands = typeof(PushNotificationsBoundedContext).Assembly
                .GetTypes()
                .Where(x => x.Namespace == "Lykke.Service.PushNotifications.Contract.Commands")
                .ToArray();

            builder.Register(ctx =>
            {
                return new CqrsEngine(_log,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngineMeRabbit,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        "messagepack",
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),

                    Register.Saga<BlockchainOperationsSaga>("blockchain-notification-saga")
                        .ListeningEvents(typeof(CashinCompletedEvent),
                                         typeof(CashoutCompletedEvent))
                        .From(Lykke.Job.BlockchainCashoutProcessor.Contract.BlockchainCashoutProcessorBoundedContext.Name).On(eventsRoute)
                        .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                        .ListeningEvents(typeof(Lykke.Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                        .From(Lykke.Job.BlockchainCashinDetector.Contract.BlockchainCashinDetectorBoundedContext.Name).On(eventsRoute)
                        .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                        .PublishingCommands(pushNotificationsCommands)
                        .To(PushNotificationsBoundedContext.Name)
                        .With(commandsRoute),

                    Register.BoundedContext(Self)
                        .PublishingCommands(pushNotificationsCommands)
                        .To(PushNotificationsBoundedContext.Name)
                        .With(commandsRoute)
                        .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256)
                    );
            })
            .Keyed<ICqrsEngine>(RabbitType.ME)
            .SingleInstance()
            .AutoActivate();

            #endregion
        }
    }
}