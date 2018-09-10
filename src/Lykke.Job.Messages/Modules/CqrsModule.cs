﻿using System.Collections.Generic;
using Autofac;
using Lykke.Job.Messages.Core;
using Lykke.SettingsReader;
using Lykke.Cqrs;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Cqrs.Configuration;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Sagas;
using Lykke.Messaging.Serialization;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PayAuth.Contract;
using Lykke.Service.PayAuth.Contract.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.Registration.Contract.Events;
using Lykke.Service.Session.Contracts;
using Lykke.Service.SwiftCredentials.Contracts;
using Lykke.Service.SwiftWithdrawal.Contracts;

namespace Lykke.Job.Messages.Modules
{
    [UsedImplicitly]
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public CqrsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string commandsRoute = "commands";
            string eventsRoute = "events";
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.MessagesJob.Transports.ClientRabbitMqConnectionString };
            var rabbitMqSagasSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.SagasRabbitMq.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();

            builder.RegisterType<BlockchainOperationsSaga>().SingleInstance();

            builder.RegisterType<TerminalSessionsSaga>().SingleInstance();
            builder.RegisterType<LoginEmailNotificationsSaga>().SingleInstance();
            builder.RegisterType<SwiftCredentialsRequestSaga>().SingleInstance();
            builder.RegisterType<LoginPushNotificationsSaga>().SingleInstance();
            builder.RegisterType<SwiftWithdrawalEmailNotificationSaga>().SingleInstance();
            builder.RegisterType<OrderExecutionSaga>().SingleInstance();
            builder.RegisterType<LykkePayOperationsSaga>().SingleInstance();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();

                    var messagingEngine = new MessagingEngine(logFactory,
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {
                                "SagasRabbitMq",
                                new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(),
                                    rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq")
                            },
                            {
                                "ClientRabbitMq",
                                new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                                    rabbitMqSettings.Password, "None", "RabbitMq")
                            },
                            {
                                "PostProcessingRabbitMq",
                                new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(),
                                    rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq")
                            }
                        }),
                        new RabbitMqTransportFactory(logFactory));

                    var sagasEndpointResolver = new RabbitMqConventionEndpointResolver(
                        "SagasRabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s");

                    var clientEndpointResolver = new RabbitMqConventionEndpointResolver(
                        "ClientRabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s");

                    var postProcessingEndpointResolver = new RabbitMqConventionEndpointResolver(
                        "PostProcessingRabbitMq",
                        SerializationFormat.ProtoBuf,
                        environment: "lykke");

                    var pushNotificationsCommands = typeof(PushNotificationsBoundedContext).Assembly
                        .GetTypes()
                        .Where(x => x.Namespace == typeof(TextNotificationCommand).Namespace)
                        .ToArray();

                    return new CqrsEngine(logFactory,
                        ctx.Resolve<IDependencyResolver>(),
                        messagingEngine,
                        new DefaultEndpointProvider(),
                        true,
                        Register.DefaultEndpointResolver(sagasEndpointResolver),

                        Register.Saga<TerminalSessionsSaga>("terminal-sessions-saga")
                            .ListeningEvents(typeof(TradingSessionCreatedEvent))
                            .From("sessions").On(eventsRoute)
                            .WithEndpointResolver(clientEndpointResolver)
                            .PublishingCommands(typeof(DataNotificationCommand))
                            .To(PushNotificationsBoundedContext.Name)
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(4).QueueCapacity(1024),

                        Register.Saga<LoginEmailNotificationsSaga>("login-email-notifications-saga")
                            .ListeningEvents(typeof(ClientLoggedEvent))
                            .From("registration").On(eventsRoute)
                            .WithEndpointResolver(clientEndpointResolver)
                            .PublishingCommands(typeof(SendEmailCommand)).To("email")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<LoginPushNotificationsSaga>("login-push-notifications-saga")
                            .ListeningEvents(typeof(ClientLoggedEvent))
                            .From("registration").On(eventsRoute)
                            .WithEndpointResolver(clientEndpointResolver)
                            .PublishingCommands(typeof(TextNotificationCommand)).To("push-notifications")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<OrderExecutionSaga>("order-execution-notifications-saga")
                            .ListeningEvents(typeof(ManualOrderTradeProcessedEvent))
                            .From(Service.PostProcessing.Contracts.Cqrs.PostProcessingBoundedContext.Name)
                            .On(eventsRoute)
                            .WithEndpointResolver(postProcessingEndpointResolver)
                            .PublishingCommands(typeof(LimitOrderNotificationCommand)).To("push-notifications")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<SwiftWithdrawalEmailNotificationSaga>("swift-withdrawal-email-notifications-saga")
                            .ListeningEvents(typeof(SwiftCashoutCreatedEvent))
                            .From(SwiftWithdrawalBoundedContext.Name).On(eventsRoute)
                            .PublishingCommands(typeof(SendEmailCommand)).To("email")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<SwiftCredentialsRequestSaga>("swift-credentials-request-saga")
                            .ListeningEvents(typeof(SwiftCredentialsRequestedEvent))
                            .From(SwiftCredentialsBoundedContext.Name).On(eventsRoute)
                            .PublishingCommands(typeof(SendEmailCommand)).To("email")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<BlockchainOperationsSaga>("blockchain-notification-saga")
                            .ListeningEvents(typeof(CashinCompletedEvent), typeof(CashoutCompletedEvent))
                            .From(BlockchainCashoutProcessor.Contract.BlockchainCashoutProcessorBoundedContext.Name)
                            .On(eventsRoute)
                            .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                            .ListeningEvents(typeof(BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                            .From(BlockchainCashinDetector.Contract.BlockchainCashinDetectorBoundedContext.Name)
                            .On(eventsRoute)
                            .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                            .PublishingCommands(pushNotificationsCommands)
                            .To(PushNotificationsBoundedContext.Name)
                            .With(commandsRoute)
                            .PublishingCommands(typeof(SendEmailCommand)).To("email")
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                        Register.Saga<LykkePayOperationsSaga>("lykkepay-employee-registration-notifications-saga")
                            .ListeningEvents(
                                typeof(EmployeeRegistrationCompletedEvent),
                                typeof(EmployeeUpdateCompletedEvent))
                            .From(EmployeeCredentialsRegistrationBoundedContext.Name)
                            .On(eventsRoute)
                            .PublishingCommands(typeof(SendEmailCommand))
                            .To(EmailMessagesBoundedContext.Name)
                            .With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256)
                    );
                })
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
