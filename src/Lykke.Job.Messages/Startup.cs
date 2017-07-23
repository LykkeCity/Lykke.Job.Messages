using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Job.Messages.Core;
using Lykke.Job.Messages.Models;
using Lykke.Job.Messages.Modules;
using Lykke.JobTriggers.Extenstions;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.Messages
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "Messages API");
            });

            var builder = new ContainerBuilder();
            var appSettings = Environment.IsDevelopment()
                ? Configuration.Get<AppSettings>()
                : HttpSettingsLoader.Load<AppSettings>(Configuration.GetValue<string>("SettingsUrl"));
            var log = CreateLogWithSlack(services, appSettings);

            builder.RegisterModule(new JobModule(appSettings, log));

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(appSettings.MessagesJob.Db.SharedStorageConnString);
            });

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLykkeMiddleware("Messages", ex => new ErrorResponse { ErrorMessage = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();

            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer.Dispose();
            });
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, AppSettings settings)
        {
            LykkeLogToAzureStorage logToAzureStorage = null;

            var logToConsole = new LogToConsole();
            var logAggregate = new LogAggregate();

            logAggregate.AddLogger(logToConsole);

            var dbLogConnectionString = settings.MessagesJob.Db.LogsConnString;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                logToAzureStorage = new LykkeLogToAzureStorage("Lykke.Job.Messages", new AzureTableStorage<LogEntity>(
                    dbLogConnectionString, "MessagesLog", logToConsole));

                logAggregate.AddLogger(logToAzureStorage);
            }

            // Creating aggregate log, which logs to console and to azure storage, if last one specified
            var log = logAggregate.CreateLogger();

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.SlackNotifications.AzureQueue.QueueName
            }, log);

            // Finally, setting slack notification for azure storage log, which will forward necessary message to slack service
            logToAzureStorage?.SetSlackNotification(slackService);

            return log;
        }
    }
}