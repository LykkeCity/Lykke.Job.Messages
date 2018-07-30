using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Job.Messages.Core;
using Lykke.SettingsReader;
using Lykke.Job.Messages.Utils;
using Lykke.Cqrs;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Cqrs.Configuration;
using System.Linq;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Job.Messages.Sagas;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;
using Lykke.Service.Registration.Contract.Events;
using Lykke.Service.Session.Contracts;
using Lykke.Service.SwiftCredentials.Contracts;
using Lykke.Service.SwiftWithdrawal.Contracts;

namespace Lykke.Job.Messages.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public CqrsModule(IReloadingManager<AppSettings> settings, ILog log)
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
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.MessagesJob.Transports.ClientRabbitMqConnectionString };
            var rabbitMqSagasSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.SagasRabbitMq.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();

            builder.RegisterType<BlockchainOperationsSaga>().SingleInstance();

            builder.RegisterType<TerminalSessionsSaga>().SingleInstance();
            builder.RegisterType<LoginEmailNotificationsSaga>().SingleInstance();
            builder.RegisterType<SwiftCredentialsRequestSaga>().SingleInstance();
            builder.RegisterType<LoginPushNotificationsSaga>().SingleInstance();
            builder.RegisterType<SwiftWithdrawalEmailNotificationSaga>().SingleInstance();

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    { "SagasRabbitMq", new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(), rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq") },
                    { "ClientRabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq") }
                }),
                new RabbitMqTransportFactory());

            var sagasEndpointResolver = new RabbitMqConventionEndpointResolver(
                "SagasRabbitMq",
                "messagepack",
                environment: "lykke",
                exclusiveQueuePostfix: "k8s");

            var clientEndpointResolver = new RabbitMqConventionEndpointResolver(
                "ClientRabbitMq",
                "messagepack",
                environment: "lykke",
                exclusiveQueuePostfix: "k8s");

            var pushNotificationsCommands = typeof(PushNotificationsBoundedContext).Assembly
                .GetTypes()
                .Where(x => x.Namespace == typeof(TextNotificationCommand).Namespace)
                .ToArray();

            builder.Register(ctx =>
              {
                  return new CqrsEngine(_log,
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
                              .From(BlockchainCashoutProcessor.Contract.BlockchainCashoutProcessorBoundedContext.Name).On(eventsRoute)
                              .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                          .ListeningEvents(typeof(BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                              .From(BlockchainCashinDetector.Contract.BlockchainCashinDetectorBoundedContext.Name).On(eventsRoute)
                              .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                          .PublishingCommands(pushNotificationsCommands)
                              .To(PushNotificationsBoundedContext.Name)
                              .With(commandsRoute)
                          .PublishingCommands(typeof(SendEmailCommand)).To("email")
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