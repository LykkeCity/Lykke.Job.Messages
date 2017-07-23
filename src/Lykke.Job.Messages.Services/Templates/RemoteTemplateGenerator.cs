using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.Messages.Core.Services.Templates;
using Lykke.Job.Messages.Services.Http;

namespace Lykke.Job.Messages.Services.Templates
{
    public class RemoteTemplateGenerator : IRemoteTemplateGenerator
    {
        private readonly string _templatesHost;
        private readonly HttpRequestClient _httpRequestClient;
        public RemoteTemplateGenerator(string templatesHost, HttpRequestClient httpRequestClient)
        {
            _templatesHost = templatesHost;
            _httpRequestClient = httpRequestClient;
        }

        public async Task<string> GenerateAsync<T>(string templateName, T templateVm)
        {
            var baseUri = new Uri(_templatesHost);
            var templateUri = new Uri(baseUri, templateName + ".html");
            var emailTemplate = GetEmailTemplate(templateUri.AbsoluteUri);
            var emailTemplateWithData = InsertData(await emailTemplate, templateVm);

            return emailTemplateWithData;
        }

        private async Task<string> GetEmailTemplate(string emailTemplateUri)
        {
            return await _httpRequestClient.GetRequest(emailTemplateUri);
        }

        private string InsertData<T>(string emailTemplate, T templateVm)
        {
            var sb = new StringBuilder(emailTemplate);

            foreach (var prop in templateVm.GetType().GetTypeInfo().GetProperties())
            {
                // in the email template, placeholders look like this: @[propertyName]
                if (prop.GetValue(templateVm, null) != null)
                    sb.Replace("@[" + prop.Name + "]", prop.GetValue(templateVm, null).ToString());
            }

            return sb.ToString();
        }
    }
}