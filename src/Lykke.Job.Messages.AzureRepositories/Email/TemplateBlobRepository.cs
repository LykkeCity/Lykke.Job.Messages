using AzureStorage;
using Lykke.Job.Messages.Core.Domain.Email;
using Lykke.Job.Messages.Core.Domain.Email.Models;
using Lykke.Service.EmailSender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlobSpace = AzureStorage;

namespace Lykke.Job.Messages.AzureRepositories.Email
{
    public class TemplateBlobRepository : ITemplateBlobRepository
    {
        private readonly string _containerName;
        private readonly IBlobStorage _storage;

        public TemplateBlobRepository(IBlobStorage storage, string containerName)
        {
            _containerName = containerName;
            _storage = storage;
        }

        public async Task<bool> CheckEmailTemplateExistsAsync(string partnerId, string templateName, string language = "EN")
        {
            var (templatePath, metadataPath) = GetHtmlAndJsonBlobNames(partnerId, templateName, language);
            bool[] result = await Task.WhenAll(_storage.HasBlobAsync(_containerName, templatePath),
                                            _storage.HasBlobAsync(_containerName, metadataPath));
            bool exists = result.Count(x => x) == 2;

            return exists;
        }

        public async Task<EmailMessage> GetEmailTemplateAsync(string partnerId, string templateName, string language = "EN")
        {
            var (templatePath, metadataPath) = GetHtmlAndJsonBlobNames(partnerId, templateName, language);

            string html = await GetBlobInfoAsync(templatePath);
            string json = await GetBlobInfoAsync(metadataPath);

            if (string.IsNullOrEmpty(html) ||
                string.IsNullOrEmpty(json))
            {
                throw new Exception($"Html: {!String.IsNullOrWhiteSpace(html)}, Json: {!String.IsNullOrWhiteSpace(html)}; Both {templatePath} and {metadataPath} should exist in {_containerName}");
            }

            EmailMetada metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailMetada>(json);

            if (metadata == null || string.IsNullOrEmpty(metadata.Subject))
            {
                throw new Exception($"{metadataPath} in {_containerName} is in wrong format");
            }

            return new EmailMessage()
            {
                Subject = metadata.Subject,
                HtmlBody = html,
                TextBody = null //TODO: Where should we use this?
            };
        }

        protected (string templatePath, string metadataPath) GetHtmlAndJsonBlobNames(string partner, string templateName, string language = "EN")
        {
            string path = $"{partner}/{language}/{templateName}";
            string templatePath = $"{path}.html";
            string metadataPath = $"{path}.json";

            return (templatePath, metadataPath);
        }

        protected async Task<string> GetBlobInfoAsync(string path)
        {
            string result = null;

            bool isBlobExistent = await _storage.HasBlobAsync(_containerName, path);
            if (isBlobExistent)
            {
                using (var stream = await _storage.GetAsync(_containerName, path))
                using (var reader = new StreamReader(stream))
                {
                    result = await reader.ReadToEndAsync();
                    return result;
                }
            }

            return await Task.FromResult((string)null);
        }
    }
}
