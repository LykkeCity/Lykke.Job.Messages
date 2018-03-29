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
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Sagas;
using Lykke.Service.EmailPartnerRouter.Contracts;
using Lykke.Service.PushNotifications.Contract;
using Lykke.Service.PushNotifications.Contract.Commands;

namespace Lykke.Job.Messages.Modules
{
    public class CqrsModule : Module
    {
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
            
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    { "MeRabbitMq", new TransportInfo(rabbitMqMeSettings.Endpoint.ToString(), rabbitMqMeSettings.UserName, rabbitMqMeSettings.Password, "None", "RabbitMq") },
                    { "ClientRabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq") }
                }),
                new RabbitMqTransportFactory());

            var meEndpointResolver = new RabbitMqConventionEndpointResolver(
                "MeRabbitMq",
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
                    Register.DefaultEndpointResolver(clientEndpointResolver),

                    Register.Saga<LoginNotificationsSaga>("login-notifications-saga")
                        .ListeningEvents(typeof(ClientLoggedEvent))
                            .From("registration").On(eventsRoute)
                        .PublishingCommands(typeof(SendEmailCommand)).To("email").With(commandsRoute)
                            .ProcessingOptions(commandsRoute).MultiThreaded(2).QueueCapacity(256),

                    Register.Saga<BlockchainOperationsSaga>("blockchain-notification-saga")
                        .ListeningEvents(typeof(CashinCompletedEvent), typeof(CashoutCompletedEvent))
                            .From(BlockchainCashoutProcessor.Contract.BlockchainCashoutProcessorBoundedContext.Name).On(eventsRoute)
                            .WithEndpointResolver(meEndpointResolver)
                            .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                        .ListeningEvents(typeof(BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))                        
                            .From(BlockchainCashinDetector.Contract.BlockchainCashinDetectorBoundedContext.Name).On(eventsRoute)
                            .WithEndpointResolver(meEndpointResolver)
                            .ProcessingOptions(eventsRoute).MultiThreaded(2).QueueCapacity(512)
                        .PublishingCommands(pushNotificationsCommands)
                            .To(PushNotificationsBoundedContext.Name)
                            .With(commandsRoute)                    
                    );
            })
            .As<ICqrsEngine>()
            .SingleInstance()
            .AutoActivate();            
        }
    }
}