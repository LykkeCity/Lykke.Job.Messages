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
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Handlers;
using Lykke.Job.Messages.Sagas;

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
            Messaging.Serialization.MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.Transports.ClientRabbitMqConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            builder.RegisterType<LoginNotificationsSaga>().SingleInstance();
            builder.RegisterType<EmailNotificationsCommandHandler>().WithParameter(TypedParameter.From(_settings.CurrentValue.EmailRetryPeriodInMinutes)).SingleInstance();

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName, rabbitMqSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory());

            builder.Register(ctx =>
            {
                return new CqrsEngine(_log,
                    ctx.Resolve<IDependencyResolver>(),
                    messagingEngine,
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        "messagepack",
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),

                    Register.Saga<LoginNotificationsSaga>("login-notifications-saga")
                        .ListeningEvents(typeof(ClientLoggedEvent)).From("registration").On("events")
                        .PublishingCommands(typeof(SendEmailCommand)).To(Self).With("commands")
                        .ProcessingOptions("commands").MultiThreaded(4).QueueCapacity(512),

                    Register.BoundedContext(Self)
                        .ListeningCommands(typeof(SendEmailCommand))
                        .On("commands")
                        .WithLoopback()
                        .WithCommandsHandler<EmailNotificationsCommandHandler>()
                        .ProcessingOptions("commands").MultiThreaded(4).QueueCapacity(512)
                    );
            })
            .As<ICqrsEngine>()
            .SingleInstance()
            .AutoActivate();
        }

        internal class AutofacDependencyResolver : IDependencyResolver
        {
            private readonly IComponentContext _context;

            public AutofacDependencyResolver(IComponentContext kernel)
            {
                _context = kernel ?? throw new ArgumentNullException(nameof(kernel));
            }

            public object GetService(Type type)
            {
                return _context.Resolve(type);
            }

            public bool HasService(Type type)
            {
                return _context.IsRegistered(type);
            }
        }
    }
}