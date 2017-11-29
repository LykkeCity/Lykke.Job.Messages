using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.Services.Http;
using Lykke.Service.TemplateFormatter;
using Lykke.Service.EmailSender;

namespace Lykke.Job.Messages.Services.Templates
{
    public class RemoteTemplateGenerator : IRemoteTemplateGenerator
    {
        private readonly ITemplateFormatter _templateFormatter;

        public RemoteTemplateGenerator(ITemplateFormatter templateFormatter)
        {
            _templateFormatter = templateFormatter;
        }

        public Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm)
        {
            return _templateFormatter.FormatAsync(templateName, partnerId, "EN", templateVm);
        }
    }
}