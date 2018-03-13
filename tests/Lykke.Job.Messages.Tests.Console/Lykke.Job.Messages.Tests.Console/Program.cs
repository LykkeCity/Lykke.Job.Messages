using Lykke.Cqrs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Job.Messages.Workflow.Commands;
using Lykke.Job.Messages.Contract.Emails.MessageData;
using Lykke.Job.Messages.Contract;

namespace Lykke.Job.Messages.Tests.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = CreateWebHost(args);
            var webHost = webHostBuilder.Build();
            var cqrsEngine = webHost.Services.GetService<ICqrsEngine>();

            cqrsEngine.SendCommand(new SendEmailCommand<NoRefundOCashOutData>()
            {
                EmailAddress = "tortyt1@gmail.com",
                MessageData = new NoRefundOCashOutData()
                {
                    Amount = 10,
                    AssetId = "ETH",
                    SrcBlockchainHash = "0x00000000000000000000000000..."
                },
                PartnerId = null
            },
            EmailMessagesBoundedContext.Name,
            EmailMessagesBoundedContext.Name);
            webHost.Run();
            
        }

        public static IWebHostBuilder CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
