using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Templates;
using RazorLight;

namespace Lykke.Job.Messages.Services.Templates
{
    public class TemplateGenerator : ITemplateGenerator
    {
        public Task<string> GenerateAsync<T>(string templateName, T templateModel)
        {
            var template = $"{templateName}.cshtml";
            var config = EngineConfiguration.Default;

            config.Namespaces.Add("Lykke.Job.Messages.Core.Extensions");

            var engine = EngineFactory.CreateEmbedded(typeof(HealthService), config);

            try
            {
                return Task.FromResult(engine.Parse(Path.GetFileNameWithoutExtension(template), templateModel));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fail template \"{template}\" compilation: {ex.Message}");
                throw;
            }
        }
    }
}
