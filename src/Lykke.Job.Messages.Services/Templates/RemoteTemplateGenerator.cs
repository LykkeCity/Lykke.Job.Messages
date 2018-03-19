using System;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Service.EmailSender;
using Lykke.Service.TemplateFormatter.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using Lykke.Job.Messages.Core.Services.Email;

namespace Lykke.Job.Messages.Services.Templates
{
    //For back compatibility only
    //Use IEmailTemplateProvider instead
    [Obsolete]
    public class RemoteTemplateGenerator : IRemoteTemplateGenerator
    {
        private readonly IEmailTemplateProvider _emailTemplateProvide;

        public RemoteTemplateGenerator(IEmailTemplateProvider emailTemplateProvide)
        {
            _emailTemplateProvide = emailTemplateProvide;
        }

        public async Task<EmailMessage> GenerateAsync<T>(string partnerId, string templateName, T templateVm)
        {
            var formatted = await _emailTemplateProvide.GenerateAsync(partnerId, templateName, "EN", templateVm);

            return formatted.EmailMessage;
        }
    }
}