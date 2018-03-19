using Lykke.Cqrs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Job.Messages.Contract;
using System.Threading;
using Lykke.Job.Messages.Events;
using Lykke.Messages.Email.MessageData;

namespace Lykke.Job.Messages.Tests.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = CreateWebHost(args);
            var webHost = webHostBuilder.Build();
            var cqrsEngine = webHost.Services.GetService<ICqrsEngine>();

            //cqrsEngine.PublishEvent(new ClientLoggedEvent()
            //{
            //    ClientId = "25c47ff8-e31e-4913-8e02-8c2512f0111e",
            //    ClientInfo = "info",
            //    Email = "",
            //    Ip = "192.168.1.127",
            //    PartnerId = null,
            //    UserAgent = "Android"

            //}, EmailMessagesBoundedContext.Name);
        }

        public static IWebHostBuilder CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
