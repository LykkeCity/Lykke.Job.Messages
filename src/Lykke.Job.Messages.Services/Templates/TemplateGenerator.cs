using System;
using System.Threading.Tasks;
using RazorLight;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Messages.Core.Services.Templates;

namespace Lykke.Job.Messages.Services.Templates
{
    public class TemplateGenerator : ITemplateGenerator
    {
        private readonly ILog _log;

        public TemplateGenerator(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
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
                _log.Warning(nameof(GenerateAsync), $"Fail template \"{template}\" compilation: {ex.Message}", ex);
                throw;
            }
        }
    }
}
