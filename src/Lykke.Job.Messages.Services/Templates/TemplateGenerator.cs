using System;
using System.Threading.Tasks;
using RazorLight;
using Common.Log;
using Lykke.Job.Messages.Core.Services.Templates;

namespace Lykke.Job.Messages.Services.Templates
{
    public class TemplateGenerator : ITemplateGenerator
    {
        private readonly ILog _log;

        public TemplateGenerator(ILog log)
        {
            _log = log;
        }

        public async Task<string> GenerateAsync<T>(string templateName, T templateModel)
        {
            var template = $"{templateName}.cshtml";
            var config = EngineConfiguration.Default;

            config.Namespaces.Add("Lykke.Job.Messages.Core.Extensions");

            var engine = EngineFactory.CreateEmbedded(templateModel.GetType(), config);

            try
            {
                return engine.Parse(templateName, templateModel);
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(
                    nameof(Messages),
                    nameof(TemplateGenerator),
                    nameof(GenerateAsync),
                    $"Fail template \"{template}\" compilation: {ex.Message}");
                throw;
            }
        }
    }
}
