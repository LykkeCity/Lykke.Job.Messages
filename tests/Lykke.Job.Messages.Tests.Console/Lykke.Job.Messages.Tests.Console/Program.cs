using Lykke.Cqrs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Job.Messages.Core.Services.Email;
using Lykke.Messages.Email.MessageData;
using Microsoft.AspNetCore.Builder;

namespace Lykke.Job.Messages.Tests.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var webHostBuilder = CreateWebHost(args);
            //var webHost = webHostBuilder.Build();
            //var emailGenerator = TestStartup.ServiceProvider.GetService<IEmailGenerator>();
            //var result = emailGenerator.GenerateNoRefundOCashOutMsg("", new NoRefundOCashOutData()
            //{
            //    AssetId = "BTC",
            //    Amount =  1.0000001,
            //    SrcBlockchainHash = "Hasj2qewweqedswwqdas"
            //
            //}).Result;
            //var cqrsEngine = TestStartup.ServiceProvider.GetService<ICqrsEngine>(); //webHost.Services.GetService<ICqrsEngine>();

            //cqrsEngine.PublishEvent(new CashinCompletedEvent()
            //{
            //    Amount = 1.0m,
            //    AssetId = "62c04960-4015-4945-bb7e-8e4a193b3653",
            //    ClientId = Guid.Parse("25c47ff8-e31e-4913-8e02-8c2512f0111e"),

            //}, EmailMessagesBoundedContext.Name);
        }

        public static IWebHostBuilder CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<TestStartup>();
    }

    public class TestStartup
    {
        public static IServiceProvider ServiceProvider;
        private readonly Startup _startup;

        public TestStartup() { 
            _startup = new Startup();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var provider = _startup.ConfigureServices(services);
            ServiceProvider = provider;

            return provider;
        }

        public void Configure(IApplicationBuilder app)
        {
            _startup.Configure(app);
        }
    }
}
