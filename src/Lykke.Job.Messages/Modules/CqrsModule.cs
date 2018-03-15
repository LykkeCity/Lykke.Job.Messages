using System;
using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.Messages.Commands;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Events;
using Lykke.Job.Messages.Handlers;
using Lykke.Job.Messages.Sagas;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.SettingsReader;

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
            Messaging.Serialization.MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.Transports.ClientRabbitMqConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            builder.RegisterType<LoginNotificationsSaga>().SingleInstance();
            builder.RegisterType<EmailNotificationsCommandHandler>().SingleInstance();

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
                            .PublishingCommands(typeof(SendEmailCommand)).To("email").With("commands"),
                        
                        Register.BoundedContext("email")
                            .ListeningCommands(typeof(SendEmailCommand))
                            .On("commands")
                            .WithLoopback()
                            .WithCommandsHandler<EmailNotificationsCommandHandler>()
                    );
                })
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }
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