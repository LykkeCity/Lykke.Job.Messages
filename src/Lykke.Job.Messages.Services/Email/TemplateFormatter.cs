using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.Job.Messages.AzureRepositories.Email;
using Lykke.Service.EmailSender;
using ITemplateFormatter = Lykke.Job.Messages.Core.Services.Email.ITemplateFormatter;

namespace Lykke.Job.Messages.Services.Email
{
    public class TemplateFormatter : ITemplateFormatter
    {
        private readonly INoSQLTableStorage<PartnerTemplateSettings> _partnerTemplateSettings;
        private readonly ILog _log;
        private static readonly Regex ParameterRegex = new Regex(@"@\[([^\]]+)\]");

        public TemplateFormatter(INoSQLTableStorage<PartnerTemplateSettings> partnerTemplateSettings, ILog log)
        {
            _partnerTemplateSettings = partnerTemplateSettings;
            _log = log;
        }

        public async Task<EmailMessage> Format(string caseId, string partnerId, string language, Dictionary<string, string> parameters)
        {
            try
            {
                var template = _partnerTemplateSettings[partnerId, $"{caseId}_{language}"];
                if (null == template)
                {
                    partnerId = "Lykke";
                    template = _partnerTemplateSettings[partnerId, $"{caseId}_{language}"];
                }
                if (null == template)
                    throw new Exception($"Unable to find email template {caseId} ({language}) for partner {partnerId}");

                string MatchEvaluator(Match match)
                {
                    var key = match.Groups[1].Value;
                    if (null != parameters && parameters.ContainsKey(key))
                        return parameters[key];
                    throw new KeyNotFoundException($"Unable to find parameter {key} required by email template {caseId} ({language}) for partner {partnerId}");
                }


                return new EmailMessage
                {
                    Subject = string.IsNullOrWhiteSpace(template.SubjectTemplate)
                        ? "testSubject"
                        : ParameterRegex.Replace(template.SubjectTemplate, MatchEvaluator),
                    TextBody = string.IsNullOrWhiteSpace(template.TextTemplateUrl)
                        ? null
                        : ParameterRegex.Replace(await LoadTemplate(template.TextTemplateUrl), MatchEvaluator),
                    HtmlBody = string.IsNullOrWhiteSpace(template.HtmlTemplateUrl)
                        ? null
                        : ParameterRegex.Replace(await LoadTemplate(template.HtmlTemplateUrl), MatchEvaluator)
                };
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(TemplateFormatter), nameof(Format), ex, DateTime.UtcNow);
                throw;
            }
        }

        private async Task<string> LoadTemplate(string templateUrl)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(templateUrl);
                if (response.StatusCode != HttpStatusCode.OK || null == response.Content)
                    throw new Exception("Template not found");
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}