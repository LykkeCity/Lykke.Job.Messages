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
using Lykke.Messaging.Contract;
using Lykke.Cqrs.Configuration;
using Lykke.Job.Messages.Contract;
using Lykke.Job.Messages.Workflow.CommandHandlers;
using Lykke.Job.Messages.Contract.Commands;

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
            builder.RegisterType<SendEmailCommandHandler>();
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

            //var emailMessageDataType = typeof(IEmailMessageData);
            //var emailTypes = emailMessageDataType.Assembly.GetTypes()
            //    .Where(type =>
            //    {
            //        var isEmailMessage = !type.IsInterface && 
            //                             !type.IsAbstract && 
            //                              emailMessageDataType.IsAssignableFrom(type);

            //        return isEmailMessage;
            //    }).ToArray();
            var boundedContext = Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(defaultRetryDelay)
                    .ListeningCommands(typeof(SendEmailCommand))
                    .On(defaultRoute)
                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024)
                    .WithCommandsHandler<SendEmailCommandHandler>()
                    .PublishingCommands(typeof(SendEmailCommand))
                    .To(Self)
                    .With(defaultPipeline);

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
                boundedContext
                );
        }
    }
}
