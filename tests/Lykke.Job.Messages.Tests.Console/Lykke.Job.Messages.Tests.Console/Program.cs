using Lykke.Cqrs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Contract;
using System.Threading;

namespace Lykke.Job.Messages.Tests.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            Thread.Sleep(30000);
            var webHostBuilder = CreateWebHost(args);
            var webHost = webHostBuilder.Build();
            var cqrsEngine = webHost.Services.GetService<ICqrsEngine>();
            var emailSender = new EmailMessageSender(cqrsEngine, EmailMessagesBoundedContext.Name);

            emailSender.SendEmail("tortyt1@gmail.com", null, new NoRefundOCashOutData()
            {
                Amount = 10,
                AssetId = "ETH",
                SrcBlockchainHash = "0x00000000000000000000000000..."
            });

            webHost.Run();
            
        }

        public static IWebHostBuilder CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
