using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Service.EmailSender;
using Lykke.Service.TemplateFormatter.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using Lykke.Job.Messages.Core.Services.Email;

namespace Lykke.Job.Messages.Services.Templates
{
    public class RemoteTemplateGenerator : IRemoteTemplateGenerator
    {
        private readonly IEmailTemplateProvide _emailTemplateProvide;

        public RemoteTemplateGenerator(IEmailTemplateProvide emailTemplateProvide)
        {
            _emailTemplateProvide = emailTemplateProvide;
        }

        public async Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(templateVm));
            var formatted = await _emailTemplateProvide.FormatAsync(templateName, partnerId, "EN", parameters);

            return formatted.EmailMessage;
        }
    }
}